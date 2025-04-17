using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ShadowSession.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ShadowSession.Helpers;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using ShadowSession.Messages;
using System.Drawing;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;

namespace ShadowSession
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly SessionOrchestrator _sessionOrchestrator;

        private MainWindow? mainWindow;

        public App()
        {
            Startup += OnApplicationStartup;
            Exit += OnApplicationExit;
            DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;

            ConfigureServices();

            using (var context = Ioc.Default.GetRequiredService<ShadowSessionContext>())
            {
                context.Database.Migrate();

                CreateOrUpdateDefaultUserSettings(context);
            }

            ExecutableDetector.Initialize();

            _sessionOrchestrator = new SessionOrchestrator();

            WeakReferenceMessenger.Default.Register<ApplicationRestartRequestMessage>(this, (recipient, message) =>
            {
                // not yet implemented
                throw new NotImplementedException();
            });
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            var taskbarIcon = new TaskbarIcon
            {
                Icon = ShadowSession.Properties.Resources.AppIcon,
                ToolTipText = "ShadowSession",
                ContextMenu = new ContextMenu
                {
                    FontSize = 12,
                    ItemsSource = new Control[]
                    {
                        new MenuItem
                        {
                            Header = "Open ShadowSession",
                            FontWeight = FontWeights.Bold,
                            Command = new RelayCommand(ShowMainWindow),
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "Exit ShadowSession",
                            Command = new RelayCommand(Shutdown),
                        }
                    }
                },
                LeftClickCommand = new RelayCommand(ShowMainWindow),
            };

            WeakReferenceMessenger.Default.Register<SendNotificationMessage>(taskbarIcon, (recipient, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var taskBarIcon = (TaskbarIcon)recipient;
                    taskbarIcon.ShowBalloonTip(message.Title, message.Text, message.Severity switch
                    {
                        NotificationSeverity.Info => BalloonIcon.Info,
                        NotificationSeverity.Warning => BalloonIcon.Warning,
                        NotificationSeverity.Error => BalloonIcon.Error,
                        _ => BalloonIcon.None,
                    });
                });
            });

            _sessionOrchestrator.Start(TimeSpan.FromSeconds(2));

#if DEBUG
            ShowMainWindow();
#endif
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            _sessionOrchestrator.Stop();

            if (mainWindow != null)
            {
                mainWindow.Close();
                mainWindow = null;
            }
        }

        private void OnApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            NotificationHelper.Notify("Unhandled Error", $"The following exception occured: {e.Exception.Message}", NotificationSeverity.Error);
            e.Handled = false;
        }



        private void ShowMainWindow()
        {
            if (mainWindow == null || mainWindow.IsClosed)
            {
                mainWindow = new MainWindow();
            }

            mainWindow.WindowState = WindowState.Maximized;
            mainWindow.Show();
        }

        private static void CreateOrUpdateDefaultUserSettings(ShadowSessionContext context)
        {
            var defaultUserSettings = new[]
            {
                    new UserSetting
                    {
                        Key = UserSettingKeys.AutomaticRecordingEnabledKey,
                        DisplayName = "Automatic Recording",
                        Description = "When set to true, program-specific values are used\n(When set to false, automatic recording is disabled globally).",
                        DefaultValue = bool.TrueString,
                        Kind = UserSettingKind.Boolean,
                        ValueRequired = true,
                        SortOrder = 0,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.RecordingsDirectoryKey,
                        DisplayName = "Recordings Output Directory",
                        Description = "The home directory for program recordings.",
                        DefaultValue = Path.Join(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                            "ShadowSession Recordings"),
                        Kind = UserSettingKind.Directory,
                        ValueRequired = true,
                        SortOrder = 1,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.DefaultRecordingsFramerateKey,
                        DisplayName = "Default Recording Frame Rate (FPS)",
                        Description = "The default recording frame rate\n(Higher values affect resource usage and recordings file size).",
                        DefaultValue = 30.ToString(),
                        Kind = UserSettingKind.Integer,
                        ValueRequired = true,
                        SortOrder = 2,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.DefaultRecordingsBitrateKey,
                        DisplayName = "Default Recording Bit Rate (Mpbs)",
                        Description = "The default recording bit rate\n(Higher values affect resource usage and recordings file size).\n480p (1-2), 720p (4-8), 1080p (8-15), 4K (20-50).",
                        DefaultValue = 8.ToString(),
                        Kind = UserSettingKind.Integer,
                        ValueRequired = true,
                        SortOrder = 3,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.NotificationsEnabledKey,
                        DisplayName = "Allow Notifications",
                        Description = "Whether or not notifications are sent (errors bypass this value).",
                        DefaultValue = bool.TrueString,
                        Kind = UserSettingKind.Boolean,
                        ValueRequired = true,
                        SortOrder = 4,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.MaxExecutableDetectionDepthKey,
                        DisplayName = "Max EXE Subfolder Depth",
                        Description = "Maximum depth to sweep when detecting program\n(Higher values increase initial loading and refresh times).",
                        DefaultValue = 5.ToString(),
                        Kind = UserSettingKind.Integer,
                        ValueRequired = true,
                        SortOrder = 5,
                        Visible = true,
                    },
                    new UserSetting
                    {
                        Key = UserSettingKeys.ShowAboutPageInitiallyKey,
                        DisplayName = "Show About Page Initially",
                        Description = "Whether or not to show the About page when the app is opened",
                        DefaultValue = bool.TrueString,
                        Kind = UserSettingKind.Boolean,
                        ValueRequired = true,
                        SortOrder = 6,
                        Visible = false, // hidden setting
                    },
                };

            foreach (var userSetting in defaultUserSettings)
            {
                var matching = context.UserSettings.SingleOrDefault(x => x.Key == userSetting.Key);

                if (matching == null)
                {
                    if (userSetting.ValueRequired && userSetting.Value == null)
                    {
                        if (userSetting.DefaultValue == null)
                        {
                            throw new Exception("If the user setting requires a value, the default value must be specified.");
                        }

                        userSetting.Value = userSetting.DefaultValue;
                    }

                    context.UserSettings.Add(userSetting);
                    continue;
                }

                if (matching.Kind != userSetting.Kind)
                {
                    matching.Value = userSetting.DefaultValue;
                }

                matching.Kind = userSetting.Kind;
                matching.ValueRequired = userSetting.ValueRequired;
                matching.SortOrder = userSetting.SortOrder;
                matching.DisplayName = userSetting.DisplayName;
                matching.DefaultValue = userSetting.DefaultValue;
                matching.Description = userSetting.Description;
                matching.Visible = userSetting.Visible;

                if (matching.ValueRequired && matching.Value == null)
                {
                    matching.Value = matching.DefaultValue;
                }
            }

            context.SaveChanges();
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // We use transient lifetime due to the very high likelyhood of concurrent calls,
            // thansk to the timer doing its own thing in the background
            services.AddDbContext<ShadowSessionContext>(options =>
            {
                options.UseSqlite();
            }, contextLifetime: ServiceLifetime.Transient);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }
    }

}
