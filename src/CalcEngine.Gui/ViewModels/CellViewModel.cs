using System.ComponentModel;
using System.Runtime.CompilerServices;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Gui.ViewModels
{
    public sealed class CellViewModel : INotifyPropertyChanged
    {
        private string _formula = string.Empty;
        private string _display = string.Empty;

        public CellAddress Address { get; }

        public string AddressText => Address.ToString();

        public string Formula
        {
            get => _formula;
            set { if (_formula != value) { _formula = value; OnPropertyChanged(); } }
        }

        public string Display
        {
            get => _display;
            set { if (_display != value) { _display = value; OnPropertyChanged(); } }
        }

        public CellViewModel(CellAddress address)
        {
            Address = address;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
