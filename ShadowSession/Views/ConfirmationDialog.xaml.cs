using MahApps.Metro.Controls;
using ShadowSession.Helpers;
using ShadowSession.ViewModels;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : MetroWindow
    {
        public ConfirmationDialog(string title, string text)
        {
            var viewModel = new ConfirmationDialogViewModel()
            {
                Title = title,
                Text = text,
            };


            viewModel.Confirmed += OnViewModelConfirmed;
            viewModel.Canceled += OnViewModelCanceled;

            DataContext = viewModel;
            InitializeComponent();
        }

        private void OnViewModelCanceled(object? sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnViewModelConfirmed(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
