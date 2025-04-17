using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ShadowSession.Messages;
using ShadowSession.ViewModels;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for ExecutablesPage.xaml
    /// </summary>
    public partial class ExecutablesPage : System.Windows.Controls.UserControl, IView
    {
        public ExecutablesPage()
        {
            DataContext = new ExecutablesPageViewModel();

            InitializeComponent();

            WeakReferenceMessenger.Default.Register<StartLoadingExecutablesPageMessage>(this, (recipient, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Cursor = System.Windows.Input.Cursors.Wait;
                });
            });

            WeakReferenceMessenger.Default.Register<StopLoadingExecutablesPageMessage>(this, (recipient, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Cursor = System.Windows.Input.Cursors.Arrow;
                });
            });
        }

        private void OnExecutablesRunningSearchTextKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ((ExecutablesPageViewModel)DataContext).SearchExecutablesRunningCommand.Execute(sender);
                e.Handled = true;
            }
        }

        private void OnExecutablesInstalledSearchTextKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ((ExecutablesPageViewModel)DataContext).SearchExecutablesInstalledCommand.Execute(sender);
                e.Handled = true;
            }
        }

        private void OnTrackExecutableManuallyButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ExecutablesPageViewModel)DataContext;

            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select an executable",
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "exe",
                Filter = "Executable files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                ValidateNames = true,
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                viewModel.TrackExecutableManuallyCommand.Execute(dialog.FileName);
            }
        }

        public void Refresh()
        {
            ((ExecutablesPageViewModel)DataContext).SoftRefreshExecutablesCommand.Execute(null);
        }
    }
}
