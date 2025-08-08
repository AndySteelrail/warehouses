using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для закрытия пикета
/// </summary>
public partial class ClosePicketViewModel : ViewModelBase
{
    private readonly IPicketService _picketService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ClosePicketViewModel> _logger;
    private readonly Picket _picket;
    
    private DateTime _closedAt = DateTime.Now;
    private string _closedAtText;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public ClosePicketViewModel(
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<ClosePicketViewModel> logger,
        Picket picket)
    {
        _picketService = picketService;
        _dialogService = dialogService;
        _logger = logger;
        _picket = picket;
        _closedAtText = _closedAt.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public string PicketName => _picket.Name;
    
    public DateTime ClosedAt
    {
        get => _closedAt;
        set => SetProperty(ref _closedAt, value);
    }
    
    public string ClosedAtText
    {
        get => _closedAtText;
        set
        {
            if (SetProperty(ref _closedAtText, value))
            {
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    _closedAt = parsedDate;
                }
            }
        }
    }
    
    public event Action<bool>? WindowClosed;
    
    [RelayCommand]
    private async Task Close()
    {
        try
        {
            LoadingOverlay.LoadingText = "Закрытие пикета...";
            LoadingOverlay.IsVisible = true;
            ClearError();

            var success = await _picketService.ClosePicketAsync(_picket.Id, ClosedAt);
            
            if (success)
            {
                _logger.LogInformation("Пикет успешно закрыт: Id={Id}, Name={Name}, ClosedAt={ClosedAt}", 
                    _picket.Id, _picket.Name, ClosedAt);
                
                // Показываем содержательное сообщение об успехе
                var message = $"Пикет '{_picket.Name}' успешно закрыт";
                if (ClosedAt != DateTime.Now)
                {
                    message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
                }
                await _dialogService.ShowMessageAsync("Успех", message);
                
                CloseWindow(true);
            }
            else
            {
                SetError("Не удалось закрыть пикет");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии пикета: {Message}", ex.Message);
            SetError($"Ошибка при закрытии пикета: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
    
    private void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }
}

