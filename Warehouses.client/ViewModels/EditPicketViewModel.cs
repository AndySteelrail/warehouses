using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для редактирования пикета
/// </summary>
public class EditPicketViewModel : EditItemViewModel
{
    private readonly IPicketService _picketService;

    public EditPicketViewModel(
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<EditPicketViewModel> logger) : base(dialogService, logger)
    {
        _picketService = picketService;
    }

    public override string WindowTitle => "Редактирование пикета";
    public override string LabelText => "Название пикета:";
    public override string WatermarkText => "Введите название пикета";
    public override string LoadingText => "Сохранение пикета...";
    public override string SuccessMessage => $"Пикет успешно переименован в '{ItemName}'";
    public override string ErrorMessageText => "Не удалось обновить пикет";

    public void Initialize(Picket picket)
    {
        base.Initialize(picket.Id, picket.Name);
    }

    protected override async Task<bool> SaveItemAsync()
    {
        return await _picketService.UpdatePicketNameAsync(ItemId, ItemName);
    }
} 