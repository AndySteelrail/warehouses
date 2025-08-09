using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для закрытия пикета
/// </summary>
public partial class ClosePicketViewModel : CloseItemViewModelBase
{
    private readonly IPicketService _picketService;
    private readonly ILogger<ClosePicketViewModel> _logger;
    private readonly Picket _picket;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public ClosePicketViewModel(
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<ClosePicketViewModel> logger,
        Picket picket) : base(dialogService)
    {
        _picketService = picketService;
        _logger = logger;
        _picket = picket;
    }
    
    public string PicketName => _picket.Name;
    public override string WindowTitle => "Закрытие пикета";
    public override string ConfirmText => "Вы действительно хотите закрыть пикет?";
    public override string TargetName => _picket.Name;
    
    public DateTime ClosedAt
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }
    
    public string ClosedAtText
    {
        get => CreatedAtText;
        set => CreatedAtText = value;
    }
    
    [RelayCommand]
    private async Task Close()
    {
        await ExecuteWithOverlayAsync(async () =>
        {
            var success = await _picketService.ClosePicketAsync(_picket.Id, GetCreatedAtUtc());
            if (!success)
            {
                throw new Exception("Не удалось закрыть пикет");
            }

            _logger.LogInformation("Пикет успешно закрыт: Id={Id}, Name={Name}, ClosedAt={ClosedAt}",
                _picket.Id, _picket.Name, ClosedAt);

            var message = $"Пикет '{_picket.Name}' успешно закрыт";
            if (ClosedAt != DateTime.Now)
            {
                message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);
            CloseWindow(true);
        }, "Закрытие пикета", "Ошибка при закрытии пикета");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }

    private async Task<bool> ExecuteWithOverlayAsync(Func<Task> action, string loadingText, string errorPrefix)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            await action();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorPrefix);
            await ShowErrorAsync($"{errorPrefix}: {ex.Message}");
            return false;
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
}

