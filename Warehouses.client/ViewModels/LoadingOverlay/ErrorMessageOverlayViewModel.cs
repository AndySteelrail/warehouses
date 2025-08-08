using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;

namespace Warehouses.client.ViewModels
{
    public class ErrorMessageOverlayViewModel : INotifyPropertyChanged
    {
        private bool _hasError;
        private string _errorMessage = string.Empty;

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public RelayCommand ClearErrorCommand { get; }

        public ErrorMessageOverlayViewModel()
        {
            ClearErrorCommand = new RelayCommand(ClearError);
        }

        public void SetError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        public void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
