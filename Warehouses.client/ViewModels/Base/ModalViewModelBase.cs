using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels.Base;

/// <summary>
/// Базовый класс для всех модальных окон
/// </summary>
public abstract class ModalViewModelBase : ViewModelBase
{
    protected readonly IDialogService _dialogService;
    
    protected ModalViewModelBase(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    
    public event Action<bool>? WindowClosed;
    
    protected void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }
    
    protected async Task ShowSuccessAsync(string title, string message)
    {
        await _dialogService.ShowMessageAsync(title, message);
    }
    
    protected async Task ShowErrorAsync(string title, string message)
    {
        await _dialogService.ShowMessageAsync(title, message);
    }
}
