using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для редактирования площадки
/// </summary>
public class EditPlatformViewModel : EditItemViewModel
{
    private readonly IPlatformService _platformService;

    public EditPlatformViewModel(
        IPlatformService platformService,
        IDialogService dialogService,
        ILogger<EditPlatformViewModel> logger) : base(dialogService, logger)
    {
        _platformService = platformService;
    }

    public override string WindowTitle => "Редактирование площадки";
    public override string LabelText => "Название площадки:";
    public override string WatermarkText => "Введите название площадки";
    public override string LoadingText => "Сохранение площадки...";
    public override string SuccessMessage => $"Площадка успешно переименована в '{ItemName}'";
    public override string ErrorMessageText => "Не удалось обновить площадку";

    public void Initialize(Platform platform)
    {
        base.Initialize(platform.Id, platform.Name);
    }

    protected override async Task<bool> SaveItemAsync()
    {
        return await _platformService.UpdatePlatformNameAsync(ItemId, ItemName);
    }
} 