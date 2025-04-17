using Microsoft.Win32;
using ShadowSession.Data;
using ShadowSession.ViewModels;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : System.Windows.Controls.UserControl, IView
    {
        public SettingsPage()
        {
            DataContext = new SettingsPageViewModel();

            InitializeComponent();
        }

        public void Refresh()
        {
            ((SettingsPageViewModel)DataContext).RefreshUserSettingsCommand.Execute(null);
        }

        private void OnUserSettingsSearchTextKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ((SettingsPageViewModel)DataContext).SearchUserSettingsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnModifyValueButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (SettingsPageViewModel)DataContext;

            if (viewModel.SelectedUserSetting == null)
            {
                return;
            }

            string? newValue = null;

            if (viewModel.SelectedUserSetting.Kind == UserSettingKind.Boolean)
            {
                bool currentValue = bool.Parse(viewModel.SelectedUserSetting.Value!);
                newValue = (!currentValue).ToString();
            }
            else if (viewModel.SelectedUserSetting.Kind == UserSettingKind.String)
            {
                var textInputDialog = new StringInputDialog("Setting Value (text):", true, viewModel.SelectedUserSetting.Value);
                if (textInputDialog.ShowDialog() == true)
                {
                    newValue = string.IsNullOrWhiteSpace(textInputDialog.GetValue()) ? null : textInputDialog.GetValue();
                }
            }
            else if (viewModel.SelectedUserSetting.Kind == UserSettingKind.Integer)
            {
                var integerInputDialog = new IntegerInputDialog(
                     "Setting Value (text):",
                     true,
                     viewModel.SelectedUserSetting.Value == null
                         ? null
                         : int.Parse(viewModel.SelectedUserSetting.Value));

                if (integerInputDialog.ShowDialog() == true)
                {
                    newValue = integerInputDialog.GetValue()?.ToString();
                }
            }
            else if (viewModel.SelectedUserSetting.Kind == UserSettingKind.Directory)
            {
                var fileDialog = new OpenFolderDialog
                {
                    FolderName = viewModel.SelectedUserSetting.Value ?? "",
                    Multiselect = false,
                    Title = "Select a directory",
                    ValidateNames = true,
                };

                if (fileDialog.ShowDialog() == true)
                {
                    newValue = string.IsNullOrWhiteSpace(fileDialog.FolderName) ? null : fileDialog.FolderName;
                }
            }
            else
            {
                throw new IndexOutOfRangeException(nameof(viewModel.SelectedUserSetting.Kind));
            }
            
            if (!viewModel.SelectedUserSetting.ValueRequired || !string.IsNullOrWhiteSpace(newValue))
            {
                viewModel.ModifyUserSettingValueCommand.Execute(newValue);
            }
        }
    }
}
