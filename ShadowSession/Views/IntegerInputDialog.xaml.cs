using MahApps.Metro.Controls;
using ShadowSession.Helpers;
using ShadowSession.ViewModels;
using System.Text.RegularExpressions;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for IntegerInputDialog.xaml
    /// </summary>
    public partial class IntegerInputDialog : MetroWindow
    {
        public IntegerInputDialog(string label, bool valueRequired, int? initialValue)
        {
            var viewModel = new IntegerInputDialogViewModel()
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

        public int? GetValue()
        {
            return ((IntegerInputDialogViewModel)DataContext).Value;
            
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

        private void OnNumericUpDownKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var castDataContext = (IntegerInputDialogViewModel)DataContext;
                if (castDataContext.SaveCommand.CanExecute(null))
                {
                    castDataContext.SaveCommand.Execute(null);
                }
                e.Handled = true;
            }

            // Prevent spaces
            else if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
