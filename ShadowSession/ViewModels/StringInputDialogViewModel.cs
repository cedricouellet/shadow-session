using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadowSession.ViewModels
{
    public class StringInputDialogViewModel : ObservableRecipient
    {
        private bool _valueRequired;

        private string? _value;

        private string? _label;

        public event EventHandler? Saved;

        public event EventHandler? Canceled;

        public StringInputDialogViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => !ValueRequired || !string.IsNullOrWhiteSpace(Value));
            CancelCommand = new RelayCommand(Cancel);
        }

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public bool ValueRequired
        {
            get => _valueRequired;
            set
            {
                if (_valueRequired != value)
                {
                    _valueRequired = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Label 
        {
            get => _label; 
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnValueChanged(_value);
                    OnPropertyChanged();
                }
            }
        }

        private void Save()
        {
            if (ValueRequired && string.IsNullOrWhiteSpace(Value))
            {
                return;
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged(string? value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
