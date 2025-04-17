using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ShadowSession.ViewModels
{
    public class ConfirmationDialogViewModel : ObservableObject
    {
        public event EventHandler? Confirmed;

        public event EventHandler? Canceled;

        private string? _title;

        private string? _text;

        public ConfirmationDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ICommand ConfirmCommand { get; set; }

        public ICommand CancelCommand { get; set; }
        
        public string? Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Text
        {
            get => _text;
            set
            {
                if (_text != value) 
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }

        }

        private void Confirm()
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }
    }
}
