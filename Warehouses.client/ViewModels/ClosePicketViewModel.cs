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
    private readonly Picket _picket;
    
    public ClosePicketViewModel(
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<ClosePicketViewModel> logger,
        Picket picket) : base(dialogService, logger)
    {
        _picketService = picketService;
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
        await ExecuteWithLoadingAsync(async () =>
        {
            var success = await _picketService.ClosePicketAsync(_picket.Id, GetCreatedAtUtc());
            if (!success)
            {
                throw new Exception("Не удалось закрыть пикет");
            }

            var message = $"Пикет '{_picket.Name}' успешно закрыт";
            if (ClosedAt != DateTime.Now)
            {
                message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);
            CloseWindow(true);
            return true;
        }, "Закрытие пикета", "Ошибка при закрытии пикета");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }


}

