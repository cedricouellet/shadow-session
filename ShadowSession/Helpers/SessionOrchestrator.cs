using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using ScreenRecorderLib;
using ShadowSession.Data;
using ShadowSession.Messages;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Timer = System.Timers.Timer;

namespace ShadowSession.Helpers
{
    public class SessionOrchestrator
    {
        private readonly Dictionary<Session, Recorder> _sessionRecorderDict = [];

        private readonly ObservableCollection<Session> _sessionsInProgress = [];

        private readonly Timer _timer;

        public SessionOrchestrator()
        {
            _sessionsInProgress.CollectionChanged += OnSessionsInProgressCollectionChanged;

            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start(TimeSpan checkInterval)
        {
            if (!Directory.Exists(LogHelper.GetRecordingLogDirectory()))
            {
                Directory.CreateDirectory(LogHelper.GetRecordingLogDirectory());
            }

            _timer.Interval = checkInterval.TotalMilliseconds;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            // Close all sessions and stop their recordings
            foreach (var session in _sessionsInProgress)
            {
                if (!context.Any<Session>(x => x.SessionId == session.SessionId))
                {
                    continue;
                }

                TryStopRecordingSession(session);

                session.EndDate = DateTime.Now;
            }

            context.SaveChanges();
        }

        private void Iterate()
        {
            var executablesRunning = ExecutableDetector.GetExecutablesRunning();
            var executableNames = executablesRunning.Select(x => x.Name.ToLower());
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var isGlobalAutomaticRecordingEnabled = UserSettingReader.IsAutomaticRecordingEnabled();
            var defaultFramerate = UserSettingReader.GetDefaultRecordingsFramerate();
            var defaultBitrate = UserSettingReader.GetDefaultRecordingsBitrate();

            // Get tracked programs that are running
            var programs = context.Programs
                .Where(x => x.IsActive)
                .Where(x => executableNames.Contains(x.Filename.ToLower()));

            // Update all tracked programs with their current paths if not matching
            foreach (var program in programs)
            {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
                var runningPath = executablesRunning.FirstOrDefault(x => x.Name.ToLower() == program.Filename.ToLower())?.Path;
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

                if (runningPath != null && runningPath != program.Path)
                {
                    context.TryAttach(program);
                    program.Path = runningPath;
                }
            }

            // Check all running sessions for expiry
            List<Session> toRemove = [];
            foreach (var session in _sessionsInProgress)
            {
                if (!context.Any<Session>(x => x.SessionId == session.SessionId))
                {
                    toRemove.Add(session);
                    continue;
                }

                var program = programs.SingleOrDefault(x => x.ProgramId == session.ProgramId);

                // If the session's program is no longer tracked or running,
                // then mark the session for removal
                if (program == null || !program.IsActive)
                {
                    context.TryAttach(session);
                    toRemove.Add(session);
                    continue;
                }

                // If automatic recording was disabled globally since the session started,
                // or the program no longer allows automatic recording,
                // then stop its recordings
                if (!isGlobalAutomaticRecordingEnabled || !program.AutomaticRecordingEnabled)
                {
                    session.Program.AutomaticRecordingEnabled = false;
                    TryStopRecordingSession(session);
                    continue;
                }
            }

            List<Session> toAdd = [];

            // Check all active and running programs
            foreach (var program in programs)
            {
                // When there are no active sessions for the program, then start a session and mark it for adding.
                if (!_sessionsInProgress.Any(x => x.ProgramId == program.ProgramId))
                {
                    var session = context.Sessions.Add(new Session
                    {
                        ProgramId = program.ProgramId,
                        StartDate = DateTime.Now
                    }).Entity;

                    context.SaveChanges();

                    toAdd.Add(session);

                    // If automatic recording is enabled, start a recording as well.
                    if (isGlobalAutomaticRecordingEnabled && program.AutomaticRecordingEnabled)
                    {
                        TryStartRecordingSession(session, defaultFramerate, defaultBitrate, context);
                    }
                }

                // When both program and global automatic recording are enabled 
                // and the program has a session that isn't recording, 
                // then start the recording
                // Use case: when toggling whether or not automatic recording is enabled while a session is active
                if (isGlobalAutomaticRecordingEnabled && program.AutomaticRecordingEnabled &&
                    _sessionsInProgress.Any(x => x.ProgramId == program.ProgramId &&
                                                 !_sessionRecorderDict.ContainsKey(x)))
                {
                    var session = _sessionsInProgress.Single(x => x.ProgramId == program.ProgramId);

                    // update values in case live program values changes
                    session.Program.RecordingBitrate = program.RecordingBitrate;
                    session.Program.RecordingFramerate = program.RecordingFramerate;
                    session.Program.AutomaticRecordingEnabled = true;

                    TryStartRecordingSession(session, defaultFramerate, defaultBitrate, context);
                }
            }

            // Remove all sessions marked for deletion from sessions in progress
            // and stop their recordings
            foreach (var session in toRemove)
            {
                _sessionsInProgress.Remove(session);

                session.EndDate = DateTime.Now;

                // Stop its recording
                TryStopRecordingSession(session);
            }


            // Add all sessions marked for adding in session in progress
            foreach (var session in toAdd)
            {
                _sessionsInProgress.Add(session);

                if (!isGlobalAutomaticRecordingEnabled && session.Program.AutomaticRecordingEnabled)
                {
                    TryStartRecordingSession(session, defaultFramerate, defaultBitrate, context);
                }
            }

            context.SaveChanges();
        }

        private Recorder? CreateRecorder(Session session, int defaultFramerate, int defaultBitrate)
        {
            // Gets the windows process window handle
            var executableWindowHandle = ExecutableDetector.GetProcessWindowHandle(session.Program.Path);

            if (executableWindowHandle == IntPtr.Zero)
            {
                return null;
            }

            // If the window handle exists, assign a recorder to the session and initialize the recording

            int framerate = session.Program.RecordingFramerate ?? defaultFramerate;
            int bitrate = (session.Program.RecordingBitrate ?? defaultBitrate) * 1000000;

            return Recorder.CreateRecorder(new RecorderOptions
            {
                SourceOptions = new SourceOptions
                {
                    RecordingSources =
                    [
                        new WindowRecordingSource(executableWindowHandle)
                    ],
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    IsFixedFramerate = true,
                    Framerate = framerate,
                    Bitrate = bitrate,
                    Quality = 100,
                    IsMp4FastStartEnabled = true,
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.Quality,
                        EncoderProfile = H264Profile.Main,
                    },
                },
                AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                },
                OutputOptions = new OutputOptions
                {
                    RecorderMode = RecorderMode.Video,
                    IsVideoCaptureEnabled = true,
                },
                LogOptions = new LogOptions
                {
                    IsLogEnabled = true,
                    LogFilePath = Path.Join(LogHelper.GetRecordingLogDirectory(), $"recorder_{DateTime.Now:yyyy-MM-dd_HHmmss}.log"),
                    LogSeverityLevel = LogLevel.Debug,
                }
            });
        }

        private bool TryStartRecordingSession(Session session, int defaultFramerate, int defaultBitrate, ShadowSessionContext context)
        {
            if (_sessionRecorderDict.ContainsKey(session))
            {
                return false;
            }

            var recorder = CreateRecorder(session, defaultFramerate, defaultBitrate);

            if (recorder == null)
            {
                return false;
            }

            recorder.OnRecordingComplete += OnSessionRecordingComplete;
            recorder.OnRecordingFailed += OnSessionRecordingFailed;

            _sessionRecorderDict[session] = recorder;

            // Use a recording number to avoid conflicts when there are multiple recordings per session
            // Example: when toggling automatic recording while a session is being recorded
            var recordingNumber = context.Recordings.Count(x => x.SessionId == session.SessionId) + 1;

            var programRecordingDirectory = DirectoryHelper.GetProgramRecordingDirectory(session.Program);

            /// Start a new recording for the session with the corresponding recording file path
            var recording = context.Recordings.Add(new Recording
            {
                SessionId = session.SessionId,
                FilePath = Path.Join(
                    programRecordingDirectory, 
                    $"{session.Program.DisplayName.Replace(" ", "_")}_{session.StartDate:yyyy-MM-dd_HHmmss}_{recordingNumber}.mp4"),
            }).Entity;

            try
            {
                // Ensure the recording directory exists for the program
                if (!Directory.Exists(programRecordingDirectory))
                {
                    Directory.CreateDirectory(programRecordingDirectory);
                }

                // Start recording
                _sessionRecorderDict[session].Record(recording.FilePath);

                return true;
            }
            catch
            {
                NotificationHelper.Notify($"Program '{session.Program.DisplayName}'", $"Could not start recording", NotificationSeverity.Error);
                
                _sessionRecorderDict.Remove(session);

                return false;
            }
        }

        private bool TryStopRecordingSession(Session session)
        {
            if (_sessionRecorderDict.TryGetValue(session, out Recorder? recorder) && recorder != null)
            {
                recorder.Stop();
                _sessionRecorderDict.Remove(session);

                return true;
            }

            return false;
        }

        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Iterate();
        }

        private void OnSessionsInProgressCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new SessionsChangedMessage(this));
        }

        private void OnSessionRecordingComplete(object? sender, RecordingCompleteEventArgs e)
        {
            NotificationHelper.Notify($"Recording Saved", $"Location: {e.FilePath}", NotificationSeverity.Info);
        }

        private void OnSessionRecordingFailed(object? sender, RecordingFailedEventArgs e)
        {
            var message = new StringBuilder()
                .AppendLine($"Location: {e.FilePath}.")
                .AppendLine($"Reason: { e.Error}")
                .ToString();

            NotificationHelper.Notify("Recording Failed", message, NotificationSeverity.Error);

            // If the recording failed, attempt to delete the recording file
            // as the content is very likely to be corrupted
            if (File.Exists(e.FilePath))
            {
                try
                {
                    File.Delete(e.FilePath);
                }
                catch
                {

                }
            }
        }

       
   
    }
}
