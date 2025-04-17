using MahApps.Metro.Controls;
using ShadowSession.ViewModels;
using System.Reflection;

namespace ShadowSession
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public bool IsClosed { get; private set; }

        public MainWindow()
        {
            var version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
            Title = $"ShadowSession {version}";

            DataContext = new MainWindowViewModel();

            Closed += OnMainWindowClose;

            InitializeComponent();
        }

        private void OnMainWindowClose(object? sender, EventArgs e)
        {
            IsClosed = true;
        }
    }
}