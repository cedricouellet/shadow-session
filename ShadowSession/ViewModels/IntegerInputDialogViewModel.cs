using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadowSession.ViewModels
{
    public class IntegerInputDialogViewModel : ObservableRecipient
    {
        private bool _valueRequired;

        private int? _value;

        private string? _label;

        public event EventHandler? Saved;

        public event EventHandler? Canceled;

        public IntegerInputDialogViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => !ValueRequired || (Value != null && Value > 0));
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

        public int? Value
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
            if (ValueRequired && (Value == null || Value <= 0))
            {
                return;
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged(int? value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
