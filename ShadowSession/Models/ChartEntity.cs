using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace ShadowSession.Models
{
    public class ChartEntity : ObservableObject
    {
        private string? _label;
        private object? _value;

        public string? Label 
        { 
            get => _label; 
            set
            {
                if (_label == value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        public object? Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChartEntityMetaData? MetaData { get; set; }
    }
}
