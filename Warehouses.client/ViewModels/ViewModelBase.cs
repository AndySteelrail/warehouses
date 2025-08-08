using CommunityToolkit.Mvvm.ComponentModel;

namespace Warehouses.client.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModels
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }
    
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
    }
    
    protected void SetError(string message)
    {
        ErrorMessage = message;
    }
}