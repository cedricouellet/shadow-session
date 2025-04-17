using MahApps.Metro.Controls;
using ShadowSession.Helpers;
using ShadowSession.ViewModels;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for StringInputDialog.xaml
    /// </summary>
    public partial class StringInputDialog : MetroWindow
    {
        public StringInputDialog(string label, bool valueRequired, string? initialValue)
        {
            var viewModel = new StringInputDialogViewModel()
            {
                Label = label,
                Value = initialValue,
                ValueRequired = valueRequired,
            };


            viewModel.Saved += OnViewModelSaved;
            viewModel.Canceled += OnViewModelCanceled;

            DataContext = viewModel;

            InitializeComponent();
        }

        public string? GetValue()
        {
            return ((StringInputDialogViewModel)DataContext).Value;
        }

        private void OnViewModelCanceled(object? sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnViewModelSaved(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var castDataContext = (StringInputDialogViewModel)DataContext;
                if (castDataContext.SaveCommand.CanExecute(null)) 
                {
                    castDataContext.SaveCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
