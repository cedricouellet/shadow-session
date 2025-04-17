using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using ShadowSession.Data;
using ShadowSession.Helpers;
using ShadowSession.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowSession.ViewModels
{
    public partial class SessionsPageViewModel : ObservableRecipient
    {
        private Session? _selectedSession;

        private bool _isSessionSelected;

        public SessionsPageViewModel()
        {
            Sessions = [];
            Sessions.CollectionChanged += delegate
            {
                OnPropertyChanged();
            };

            RefreshSessionsCommand = new RelayCommand(RefreshSessions);

            OpenRecordingFileCommand = new RelayCommand<int?>(OpenRecordingFile);

            Messenger.Register<SessionsChangedMessage>(this, (recipient, message) =>
            {
                DispatchHelper.InvokeDispatch(RefreshSessions);
            });
        }

        public IRelayCommand RefreshSessionsCommand { get; }

        public IRelayCommand<int?> OpenRecordingFileCommand { get; }

        public ObservableCollection<Session> Sessions { get; }

        public Session? SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (_selectedSession?.SessionId != value?.SessionId)
                {
                    _selectedSession = value;
                    OnSelectedProgramChanged();
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSessionSelected
        {
            get => _isSessionSelected;
            set
            {
                if (_isSessionSelected != value)
                {
                    _isSessionSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnSelectedProgramChanged()
        {
            IsSessionSelected = SelectedSession != null;
        }

        private void RefreshSessions()
        {
            var selectedId = SelectedSession?.SessionId;

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            Sessions.Clear();

            

            var sessions = context.Sessions.AsNoTracking()
                .Where(x => x.Program.IsActive)
                .Include(x => x.Program)
                .Include(x => x.Recordings)
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.ProgramId)
                .AsEnumerable();

            foreach (var session in sessions)
            {
                foreach (var recording in session.Recordings)
                {
                    if (!Path.Exists(recording.FilePath))
                    {
                        context.TryAttach(recording);
                        context.Recordings.Remove(recording);
                        context.SaveChanges();

                        session.Recordings.Remove(recording);
                    }
                }

                Sessions.Add(session);
            }

            if (selectedId != null)
            {
                SelectedSession = Sessions.SingleOrDefault(x => x.SessionId == selectedId);
            }
        }

        private void OpenRecordingFile(int? recordingId)
        {
            if (recordingId == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var recording = context.Recordings.Single(x => x.RecordingId == recordingId);

            if (!Path.Exists(recording.FilePath))
            {
                NotificationHelper.Notify("Recording Error", "The file no longer exists", NotificationSeverity.Error);
                context.Recordings.Remove(recording);
                context.SaveChanges();
                RefreshSessions();
                return;
            }

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = recording.FilePath,
                    UseShellExecute = true,
                }
            };

            process.Start();
        }
    }
}
