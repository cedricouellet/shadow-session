using CommunityToolkit.Mvvm.Messaging;
using ShadowSession.Data;
using ShadowSession.Messages;
using ShadowSession.ViewModels;
using System.Text;
using System.Windows.Controls;

namespace ShadowSession.Views
{

    /// <summary>
    /// Interaction logic for ProgramsPage.xaml
    /// </summary>
    public partial class ProgramsPage : System.Windows.Controls.UserControl, IView
    {
        public ProgramsPage()
        {
            DataContext = new ProgramsPageViewModel();
            InitializeComponent();
        }

        public void Refresh()
        {
            var viewModel = ((ProgramsPageViewModel)DataContext);
            viewModel.RefreshProgramsCommand.Execute(null);
        }

        private void OnProgramsSearchTextKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ((ProgramsPageViewModel)DataContext).SearchProgramsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnProgramsDataGridMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                ((DataGrid)sender).UnselectAll();
            }
        }

        private void OnRemoveButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ProgramsPageViewModel)DataContext;

            if (viewModel.SelectedProgram == null)
            {
                return;
            }

            var message = new StringBuilder()
              .AppendLine($"You chose to stop tracking the program '{viewModel.SelectedProgram.DisplayName}'.")
              .AppendLine()
              .AppendLine("This action will not delete any existing data, sessions, or recordings.")
              .AppendLine()
              .AppendLine("However, the program and its sessions will be hidden, excluded from statistics, and future sessions will no longer be tracked.")
              .AppendLine()
              .AppendLine("You can add the program back to tracking later if needed.")
              .AppendLine()
              .AppendLine("Do you want to continue?")
              .ToString();

            var confirmationDialog = new ConfirmationDialog("Remove Program from Tracking", message);
            if (confirmationDialog.ShowDialog() == true)
            {
                viewModel.RemoveProgramCommand.Execute(null);
            }
        }

        private void OnRenameButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ProgramsPageViewModel)DataContext;

            if (viewModel.SelectedProgram == null)
            {
                return;
            }

            var inputDialog = new StringInputDialog("Program Name:", true, viewModel.SelectedProgram.DisplayName);
            if (inputDialog.ShowDialog() == true)
            {
                viewModel.RenameProgramCommand.Execute(inputDialog.GetValue());
            }
        }

        private void OnChangeRecordingFramerateButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ProgramsPageViewModel)DataContext;

            if (viewModel.SelectedProgram == null)
            {
                return;
            }

            var inputDialog = new IntegerInputDialog("Program Recording Frame Rate (FPS):", true, viewModel.SelectedProgram.RecordingFramerate);
            if (inputDialog.ShowDialog() == true)
            {
                viewModel.ModifyProgramRecordingFramerateCommand.Execute(inputDialog.GetValue());
            }
        }

        private void OnChangeRecordingBitrateButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ProgramsPageViewModel)DataContext;

            if (viewModel.SelectedProgram == null)
            {
                return;
            }

            var inputDialog = new IntegerInputDialog("Program Recording Bit Rate (Mbps):", true, viewModel.SelectedProgram.RecordingBitrate);
            if (inputDialog.ShowDialog() == true)
            {
                viewModel.ClearProgramRecordingBitrateCommand.Execute(inputDialog.GetValue());
            }
        }
    }
}
