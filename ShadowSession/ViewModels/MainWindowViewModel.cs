using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using ShadowSession.Data;
using ShadowSession.Helpers;
using ShadowSession.Messages;
using ShadowSession.Views;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace ShadowSession.ViewModels
{
    public partial class MainWindowViewModel : ObservableRecipient
    {
        private readonly Dictionary<Type, UserControl> _views = [];

        private UserControl _activeView;

        private int _totalPrograms = 0;
        private int _totalSessions = 0;
        private double _averageSessionsPerProgram = 0;

        private TimeSpan _totalSessionDuration = TimeSpan.Zero;
        private TimeSpan _averageSessionDuration = TimeSpan.Zero;
        private TimeSpan _lastTwoWeeksSessionDuration = TimeSpan.Zero;

        public MainWindowViewModel()
        {
            if (UserSettingReader.ShouldShowAboutPageInitially())
            {
                _activeView = new AboutPage();
                _views[typeof(AboutPage)] = _activeView;

                UserSettingReader.SetShowAboutPageInitially(false);
            }
            else
            {
                _activeView = new OverviewPage();
                _views[typeof(OverviewPage)] = _activeView;
            }
   
            SetActiveViewTypeCommand = new RelayCommand<Type>(SetActiveView, new Predicate<Type?>(CanSetActiveView));

            ShowGuidePageCommand = new RelayCommand(() => SetActiveView(typeof(GuidePage)));

            RefreshUpdateStatistics();

            Messenger.Register<ProgramsChangedMessage>(this, (recipient, message) =>
            {
                RefreshUpdateStatistics();
            });


            Messenger.Register<SessionsChangedMessage>(this, (recipient, message) =>
            {
                RefreshUpdateStatistics();
            });
        }

        public IRelayCommand<Type> SetActiveViewTypeCommand { get; }

        public IRelayCommand ShowGuidePageCommand { get; }

        public UserControl ActiveView 
        {
            get => _activeView;
            private set 
            {
                if (_activeView != value)
                {
                    _activeView = value;
                    OnActiveViewChanged(_activeView);
                    OnPropertyChanged();
                }
            }
        }

        public int TotalProgramCount 
        {
            get => _totalPrograms;
            private set
            {
                if (_totalPrograms != value)
                {
                    _totalPrograms = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalSessionCount
        {
            get => _totalSessions;
            private set
            {
                if (_totalSessions != value)
                {
                    _totalSessions = value;
                    OnPropertyChanged();
                }
            }
        }

        public double AverageSessionsPerProgram
        {
            get => _averageSessionsPerProgram;
            private set
            {
                if (_averageSessionsPerProgram != value)
                {
                    _averageSessionsPerProgram = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan AverageSessionDuration
        {
            get => _averageSessionDuration;
            private set 
            {
                if (_averageSessionDuration != value)
                {
                    _averageSessionDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan TotalSessionDuration
        {
            get => _totalSessionDuration;
            private set
            {
                if (_totalSessionDuration != value)
                {
                    _totalSessionDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan LastTwoWeeksSessionDuration
        {
            get => _lastTwoWeeksSessionDuration;
            private set
            {
                if (_lastTwoWeeksSessionDuration != value)
                {
                    _lastTwoWeeksSessionDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        private void SetActiveView(Type? viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentException("The view type cannot be null");
            }

            if (!_views.TryGetValue(viewType, out UserControl? page) || page == null)
            {
                page = (UserControl?)Activator.CreateInstance(viewType);

                if (page == null)
                {
                    throw new Exception("Could not instantiate page");
                }
            }

            ActiveView = page!;
        }

        private bool CanSetActiveView(Type? type)
        {
            return type != ActiveView.GetType();
        }

        private void RefreshUpdateStatistics()
        {
            TotalProgramCount = 0;
            TotalSessionCount = 0;
            AverageSessionsPerProgram = 0;
            AverageSessionDuration = TimeSpan.Zero;
            TotalSessionDuration = TimeSpan.Zero;

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var programs = context.Programs
                .Where(x => x.IsActive)
                .AsNoTracking()
                .Include(x => x.Sessions);

            if (programs.Any())
            {
                TotalProgramCount = programs.Count();
                
                TotalSessionCount = programs
                    .Select(x => x.TotalSessionCount)
                    .AsEnumerable()
                    .Sum();

                AverageSessionsPerProgram = Math.Round((double)programs
                    .Select(x => x.TotalSessionCount)
                    .AsEnumerable()
                    .Average(), 2);

                LastTwoWeeksSessionDuration = TimeSpan.FromSeconds(programs
                    .Select(x => x.LastTwoWeeksSessionDuration.TotalSeconds)
                    .AsEnumerable()
                    .Sum());

                AverageSessionDuration = TimeSpan.FromSeconds(programs
                    .Select(x => x.AverageSessionDuration.TotalSeconds)
                    .AsEnumerable()
                    .Average());

                TotalSessionDuration = TimeSpan.FromSeconds(programs
                    .Select(x => x.TotalSessionDuration.TotalSeconds)
                    .AsEnumerable()
                    .Sum());
            } 
        }

        private void OnActiveViewChanged(UserControl value)
        {
            _views[_activeView.GetType()] = value;

            if (value is IView view)
            {
                view.Refresh();
            }
            SetActiveViewTypeCommand.NotifyCanExecuteChanged();
        }

    }
}
