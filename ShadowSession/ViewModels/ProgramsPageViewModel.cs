using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using ShadowSession.Data;
using ShadowSession.Helpers;
using ShadowSession.Messages;
using SkiaSharp.Views.WPF;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace ShadowSession.ViewModels
{
    public partial class ProgramsPageViewModel : ObservableRecipient
    {
        private string? _programsSearchText;

        private IEnumerable<ISeries> _programTotalSessionHoursSeries = [];
        private IEnumerable<ISeries> _programRecordedSessionSeries = [];
        private IEnumerable<ISeries> _programSessionHoursOverTimeSeries = [];

        private Program? _selectedProgram;

        private bool _isProgramSelected;

        public ProgramsPageViewModel()
        {
            Programs = [];
            Programs.CollectionChanged += delegate
            {
                OnProgramsChanged();
                OnPropertyChanged(nameof(Programs));
            };

            RefreshProgramsCommand = new RelayCommand(RefreshPrograms);

            SearchProgramsCommand = new RelayCommand(SearchPrograms);

            ResetProgramsSearchTextCommand = new RelayCommand(() =>
            {
                ProgramsSearchText = null;
                SearchPrograms();
            });

            RemoveProgramCommand = new RelayCommand(RemoveProgram);

            RenameProgramCommand = new RelayCommand<string?>(RenameProgram);

            ToggleProgramAllowRecordingCommand = new RelayCommand(ToggleProgramAllowRecording);

            OpenProgramRecordingsDirectoryCommand = new RelayCommand(OpenProgramRecordingsDirectory);

            ModifyProgramRecordingFramerateCommand = new RelayCommand<int?>(ModifyProgramRecordingFramerate);

            ClearProgramRecordingFramerateCommand = new RelayCommand(ClearProgramRecordingFramerate);

            ModifyProgramRecordingBitrateCommand = new RelayCommand<int?>(ModifyProgramRecordingBitrate);

            ClearProgramRecordingBitrateCommand = new RelayCommand(ClearProgramRecordingBitrate);

            Messenger.Register<ProgramsChangedMessage>(this, (recipient, message) =>
            {
                DispatchHelper.InvokeDispatch(RefreshPrograms);
            });

            Messenger.Register<SessionsChangedMessage>(this, (recipient, message) =>
            {
                DispatchHelper.InvokeDispatch(RefreshPrograms);
            });
        }

        public IRelayCommand RefreshProgramsCommand { get; } 

        public IRelayCommand SearchProgramsCommand { get; }

        public IRelayCommand ResetProgramsSearchTextCommand { get; }

        public IRelayCommand RemoveProgramCommand { get; }

        public IRelayCommand<string> RenameProgramCommand { get; }

        public IRelayCommand ToggleProgramAllowRecordingCommand { get; }

        public IRelayCommand OpenProgramRecordingsDirectoryCommand { get; }

        public IRelayCommand<int?> ModifyProgramRecordingFramerateCommand { get; }

        public IRelayCommand ClearProgramRecordingFramerateCommand { get; }

        public IRelayCommand<int?> ModifyProgramRecordingBitrateCommand { get; }

        public IRelayCommand ClearProgramRecordingBitrateCommand { get; }

        public ObservableCollection<Program> Programs { get; }

        public IEnumerable<ISeries> ProgramTotalSessionHoursSeries
        {
            get => _programTotalSessionHoursSeries;
            private set
            {
                if (_programTotalSessionHoursSeries != value)
                {
                    _programTotalSessionHoursSeries = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<ISeries> ProgramRecordedSessionSeries
        {
            get => _programRecordedSessionSeries;
            private set
            {
                if (_programRecordedSessionSeries != value)
                {
                    _programRecordedSessionSeries = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<ISeries> ProgramSessionHoursOverTimeSeries
        {
            get => _programSessionHoursOverTimeSeries;
            set
            {
                if (_programSessionHoursOverTimeSeries != value)
                {
                    _programSessionHoursOverTimeSeries = value;
                    OnPropertyChanged();
                }
            }
        }

        public Axis[] ProgramSessionHoursOverTimeXAxes { get; } = [
            new Axis 
            { 
                Name = "Months", 
                Labels = Enumerable.Range(1, 12).Select(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName).ToList(),
                LabelsPaint = new SolidColorPaint(ThemeColors.Gray.ToSKColor()),
                NamePaint = new SolidColorPaint(ThemeColors.Gray.ToSKColor()),
            }];

        public Axis[] ProgramSessionHoursOverTimeYAxes { get; } = [
            new Axis 
            { 
                Name = "Hours",
                LabelsPaint = new SolidColorPaint(ThemeColors.Gray.ToSKColor()),
                NamePaint = new SolidColorPaint(ThemeColors.Gray.ToSKColor()),
            }];

        public Brush ForegroundBrush { get; } = new SolidColorBrush(ThemeColors.ThemeForeground);

        public Paint ForegroundPaint { get; } = new SolidColorPaint(ThemeColors.ThemeForeground.ToSKColor());

        public string? ProgramsSearchText
        {
            get => _programsSearchText;
            set
            {
                if (_programsSearchText != value)
                {
                    _programsSearchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public Program? SelectedProgram
        {
            get => _selectedProgram;
            set
            {
                if (_selectedProgram?.ProgramId != value?.ProgramId)
                {
                    _selectedProgram = value;
                    OnSelectedProgramChanged();
                    OnPropertyChanged();
                }
            }
        }

        public bool IsProgramSelected
        {
            get => _isProgramSelected;
            private set
            {
                if (_isProgramSelected != value)
                {
                    _isProgramSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RefreshCharts()
        {
            var totalSessionsHours = Programs.Sum(x => x.TotalSessionDuration.TotalHours);
            var percentageSessionHours = Math.Round(100.0 * (SelectedProgram?.TotalSessionDuration.TotalHours ?? 0 / (double)totalSessionsHours), 2);

            ProgramTotalSessionHoursSeries = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(percentageSessionHours, series =>
                {
                    series.MaxRadialColumnWidth = 12.5;
                    series.DataLabelsSize = 20;
                    series.DataLabelsFormatter = x => $"{x.StackedValue?.Total}%";
                    series.DataLabelsPaint = new SolidColorPaint(ThemeColors.ThemeForeground.ToSKColor());
                    series.Fill = new SolidColorPaint(ThemeColors.Accent.ToSKColor());
                }),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.Fill = new SolidColorPaint(ThemeColors.Accent4.ToSKColor());
                }));

            var hasRecordings = SelectedProgram?.Sessions.Any(x => x.Recordings.Any()) == true;
            var percentageSessionsRecorded = hasRecordings 
                ? Math.Round(100.0 * ((SelectedProgram?.Sessions.Where(x => x.Recordings.Any()).Count() ?? 0) / (double)(SelectedProgram?.TotalSessionCount ?? 0)))
                : 0;

            ProgramRecordedSessionSeries = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(percentageSessionsRecorded, series =>
                {
                    series.MaxRadialColumnWidth = 12.5;
                    series.DataLabelsSize = 20;
                    series.DataLabelsFormatter = x => $"{x.StackedValue?.Total}%";
                    series.DataLabelsPaint = new SolidColorPaint(ThemeColors.IdealForeground.ToSKColor());
                    series.Fill = new SolidColorPaint(ThemeColors.Accent.ToSKColor());
                }),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.Fill = new SolidColorPaint(ThemeColors.Accent4.ToSKColor());
                }));

            var sessionsForYear = SelectedProgram?.Sessions.Where(x => x.StartDate.Year == 2024 && x.Duration != TimeSpan.Zero)
                .GroupBy(x => x.StartDate.Month)
                .Select(x => new
                {
                    Month = x.Key,
                    TotalHours = Math.Round(x.Sum(s => s.Duration.TotalHours), 2),
                    AverageHours = Math.Round(x.Average(s => s.Duration.TotalHours), 2),
                })
                .OrderBy(x => x.Month)
                .ToList() ?? [];

            var months = Enumerable.Range(1, 12).ToList();
            var monthsLabels = months.Select(x => x.ToString()).ToList();

            var totalHours = months
                .Select(month => sessionsForYear.FirstOrDefault(x => x.Month == month)?.TotalHours ?? 0).ToList();

            var averageHours = months
                .Select(month => sessionsForYear.FirstOrDefault(x => x.Month == month)?.AverageHours ?? 0).ToList();

            ProgramSessionHoursOverTimeSeries = [
                new LineSeries<double, CircleGeometry>
                {
                    Name = "Average",
                    Values = averageHours.ToList(),
                    DataLabelsPaint = new SolidColorPaint(ThemeColors.ThemeForeground.ToSKColor()),
                    Stroke = new SolidColorPaint(ThemeColors.Gray.ToSKColor(), 4),
                    Fill = new SolidColorPaint(ThemeColors.GraySemiTransparent.ToSKColor(), 4),
                    GeometryFill = new SolidColorPaint(ThemeColors.ThemeBackground.ToSKColor()),
                    GeometryStroke = new SolidColorPaint(ThemeColors.Gray.ToSKColor(), 3),
                },
                new LineSeries<double, CircleGeometry> 
                {
                    Name = "Total",
                    Values = totalHours.ToList(),
                    DataLabelsPaint = new SolidColorPaint(ThemeColors.ThemeForeground.ToSKColor()),
                    Stroke = new SolidColorPaint(ThemeColors.Accent.ToSKColor(), 4),
                    Fill = new SolidColorPaint(ThemeColors.Accent4.ToSKColor(), 4),
                    GeometryFill = new SolidColorPaint(ThemeColors.ThemeBackground.ToSKColor()),
                    GeometryStroke = new SolidColorPaint(ThemeColors.Accent.ToSKColor(), 3),
                }];
        }

        private void RefreshPrograms()
        {
            ProgramsSearchText = null;

            var selectedId = SelectedProgram?.ProgramId;

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            Programs.Clear();

            var programs = context.Programs.AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.Sessions)
                .ThenInclude(x => x.Recordings)
                .AsEnumerable();

            foreach (var program in programs)
            {
                Programs.Add(program);
            }
      
            if (selectedId != null)
            {
                SelectedProgram = Programs.SingleOrDefault(x => x.ProgramId == selectedId);
            }
        }

        private void SearchPrograms()
        {
            Programs.Clear();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var programs = context.Programs.AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.Sessions)
                .ThenInclude(x => x.Recordings)
                .Where(x => string.IsNullOrWhiteSpace(ProgramsSearchText) || 
                            x.DisplayName.ToLower().Contains(ProgramsSearchText.Trim().ToLower()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var program in programs)
            {
                Programs.Add(program);
            }
        }

        private void RemoveProgram()
        {
            if (SelectedProgram == null)
            {
                return;
            }
            
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();
            context.TryAttach(SelectedProgram);

            SelectedProgram.IsActive = false;
            SelectedProgram.AutomaticRecordingEnabled = false;
            
            Programs.Remove(SelectedProgram);

            context.SaveChanges();
            
            Messenger.Send(new ProgramsChangedMessage(this));

            SelectedProgram = null;
        }

        private void RenameProgram(string? newName)
        {
            if (string.IsNullOrWhiteSpace(newName) || SelectedProgram == null) 
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedProgram);

            SelectedProgram.DisplayName = newName.Trim();

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void ToggleProgramAllowRecording()
        {
            if (SelectedProgram == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedProgram);

            SelectedProgram.AutomaticRecordingEnabled = !SelectedProgram.AutomaticRecordingEnabled;

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void OpenProgramRecordingsDirectory()
        {
            if (SelectedProgram == null)
            {
                return;
            }

            var directory = DirectoryHelper.GetProgramRecordingDirectory(SelectedProgram);

            if (!Path.Exists(directory))
            {
                NotificationHelper.Notify($"Program '{SelectedProgram.DisplayName}'", "Recordings directory not found", NotificationSeverity.Warning);
                return;
            }

            Process.Start("explorer.exe", directory);
        }

        private void ModifyProgramRecordingFramerate(int? newFramerate)
        {
            if (SelectedProgram == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedProgram);

            SelectedProgram.RecordingFramerate = newFramerate;

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void ClearProgramRecordingFramerate()
        {
            if (SelectedProgram == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();
            context.TryAttach(SelectedProgram);

            SelectedProgram.RecordingFramerate = null;

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void ModifyProgramRecordingBitrate(int? newBitrate)
        {
            if (SelectedProgram == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedProgram);

            SelectedProgram.RecordingBitrate = newBitrate;

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void ClearProgramRecordingBitrate()
        {
            if (SelectedProgram == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();
            context.TryAttach(SelectedProgram);

            SelectedProgram.RecordingBitrate = null;

            context.SaveChanges();

            Messenger.Send(new ProgramsChangedMessage(this));
        }

        private void OnProgramsChanged()
        {
            RefreshCharts();
        }

        private void OnSelectedProgramChanged()
        {
            IsProgramSelected = SelectedProgram != null;

            if (SelectedProgram != null)
            {
                using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();
                foreach (var session in SelectedProgram.Sessions)
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
                }
            }
       
            RefreshCharts();
        }
    }
}
