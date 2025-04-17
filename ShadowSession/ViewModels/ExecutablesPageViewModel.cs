using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ShadowSession.Data;
using ShadowSession.Helpers;
using ShadowSession.Messages;
using System.Collections.ObjectModel;

namespace ShadowSession.ViewModels
{
    public partial class ExecutablesPageViewModel : ObservableRecipient
    {
        private string? _executablesInstalledSearchText;

        private string? _executablesRunningSearchText;

        private Executable? _selectedExecutableInstalled;

        private Executable? _selectedExecutableRunning;

        public ExecutablesPageViewModel()
        {
            ExecutablesInstalled = [];
            ExecutablesInstalled.CollectionChanged += delegate
            {
                OnPropertyChanged(nameof(ExecutablesInstalled));
            };

            ExecutablesRunning = [];
            ExecutablesRunning.CollectionChanged += delegate
            {
                OnPropertyChanged(nameof(ExecutablesRunning));
            };

            SoftRefreshExecutablesCommand = new RelayCommand(() => RefreshExecutables(false));
            
            HardRefreshExecutablesCommand = new RelayCommand(() => RefreshExecutables(true));

            SearchExecutablesInstalledCommand = new RelayCommand(SearchExecutablesInstalled);
            SearchExecutablesRunningCommand = new RelayCommand(SearchExecutablesRunning);

            TrackExecutableRunningCommand = new RelayCommand(() => TrackExecutable(SelectedExecutableRunning));
            TrackExecutableInstalledCommand = new RelayCommand(() => TrackExecutable(SelectedExecutableInstalled));

            TrackExecutableManuallyCommand = new RelayCommand<string?>(TrackExecutableManually);

            ResetExecutablesInstalledSearchTextCommand = new RelayCommand(() =>
            {
                ExecutablesInstalledSearchText = null;
                SearchExecutablesInstalled();
            });

            ResetExecutablesRunningSearchTextCommand = new RelayCommand(() =>
            {
                ExecutablesRunningSearchText = null;
                SearchExecutablesRunning();
            });
        }
     
        public IRelayCommand TrackExecutableRunningCommand { get; }

        public IRelayCommand TrackExecutableInstalledCommand { get; }

        public IRelayCommand<string?> TrackExecutableManuallyCommand { get; }

        public IRelayCommand SoftRefreshExecutablesCommand { get; }

        public IRelayCommand HardRefreshExecutablesCommand { get; }

        public IRelayCommand ResetExecutablesInstalledSearchTextCommand { get; }

        public IRelayCommand ResetExecutablesRunningSearchTextCommand { get; }

        public IRelayCommand SearchExecutablesInstalledCommand { get; }

        public IRelayCommand SearchExecutablesRunningCommand { get; }

        public ObservableCollection<Executable> ExecutablesInstalled { get; }

        public ObservableCollection<Executable> ExecutablesRunning { get; }

        public Executable? SelectedExecutableInstalled
        {
            get => _selectedExecutableInstalled;
            set
            {
                if (_selectedExecutableInstalled != value) 
                {
                    _selectedExecutableInstalled = value;
                    OnPropertyChanged();
                }
            }
        }

        public Executable? SelectedExecutableRunning
        {
            get => _selectedExecutableRunning;
            set
            {
                if (_selectedExecutableRunning != value)
                {
                    _selectedExecutableRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ExecutablesInstalledSearchText
        {
            get => _executablesInstalledSearchText;
            set
            {
                if (_executablesInstalledSearchText != value)
                {
                    _executablesInstalledSearchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ExecutablesRunningSearchText
        {
            get => _executablesRunningSearchText;
            set
            {
                if (_executablesRunningSearchText != value)
                {
                    _executablesRunningSearchText = value;
                    OnPropertyChanged();
                }
            }
        }

        private void TrackExecutable(Executable? executable)
        {
            if (executable == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            // Check if a deactivated program already exists for this executable
            var program = context.Programs.Where(x => !x.IsActive).FirstOrDefault(x => x.Filename == executable.Name || x.Path == executable.Path);
            
            // If it does not exist, create it
            program ??= context.Programs.Add(new Program
                {
                    DisplayName = executable.Name,
                    AutomaticRecordingEnabled = false,
                }).Entity;

            // Set its updated values
            program.Filename = executable.Name;
            program.Path = executable.Path;
            program.IsActive = true;

            context.SaveChanges();

            var potentialInstalledExecutable = ExecutablesInstalled.FirstOrDefault(x => x.Name.Equals(executable.Name, StringComparison.CurrentCultureIgnoreCase));
            if (potentialInstalledExecutable != null)
            {
                ExecutablesInstalled.Remove(potentialInstalledExecutable);
            }

            var potentialRunningExecutable = ExecutablesRunning.FirstOrDefault(x => x.Name.Equals(executable.Name, StringComparison.CurrentCultureIgnoreCase));
            if (potentialRunningExecutable != null)
            {
                ExecutablesRunning.Remove(potentialRunningExecutable);
            }

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void TrackExecutableManually(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            TrackExecutable(new Executable
            {
                Name = filePath.Split(@"\", StringSplitOptions.TrimEntries).Last().Replace(".exe", ""),
                Path = filePath,
            });
        }

        private void RefreshExecutables(bool invalidateCache = false)
        {
            ExecutablesInstalledSearchText = null;
            ExecutablesRunningSearchText = null;

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();
            
            var programs = context.Programs.Where(x => x.IsActive).AsNoTracking();

            Messenger.Send(new StartLoadingExecutablesPageMessage(this));

            ExecutablesInstalled.Clear();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var executablesInstalled = ExecutableDetector.GetExecutablesInstalled(invalidateCache)
                .Where(x => !programs.Any(g => g.Path.ToLower() == x.Path.ToLower()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var executable in executablesInstalled)
            {
                ExecutablesInstalled.Add(executable);
            }

            ExecutablesRunning.Clear();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var executablesRunning = ExecutableDetector.GetExecutablesRunning()
                .Where(x => !programs.Any(g => g.Path.ToLower() == x.Path.ToLower()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var executable in executablesRunning)
            {
                ExecutablesRunning.Add(executable);
            }

            Messenger.Send(new StopLoadingExecutablesPageMessage(this));
        }

        private void SearchExecutablesInstalled()
        {
            ExecutablesInstalled.Clear();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var programs = context.Programs.Where(x => x.IsActive).AsNoTracking();


#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var executablesInstalled = ExecutableDetector.GetExecutablesInstalled()
                .Where(x => !programs.Any(g => g.Path.ToLower() == x.Path.ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(ExecutablesInstalledSearchText) || 
                            x.Name.ToLower().Contains(ExecutablesInstalledSearchText.Trim().ToLower()) || 
                            x.Path.ToLower().Contains(ExecutablesInstalledSearchText.Trim().ToLower()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var executable in executablesInstalled)
            {
                ExecutablesInstalled.Add(executable);
            }
        }

        private void SearchExecutablesRunning()
        {
            ExecutablesRunning.Clear();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var programs = context.Programs.Where(x => x.IsActive).AsNoTracking();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var executablesInstalled = ExecutableDetector.GetExecutablesRunning()
                .Where(x => !programs.Any(g => g.Path.ToLower() == x.Path.ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(ExecutablesRunningSearchText) ||
                            x.Name.ToLower().Contains(ExecutablesRunningSearchText.Trim().ToLower()) ||
                            x.Path.ToLower().Contains(ExecutablesRunningSearchText.Trim().ToLower()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var executable in executablesInstalled)
            {
                ExecutablesRunning.Add(executable);
            }
        }
    }
}
