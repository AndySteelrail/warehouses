using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для редактирования склада
/// </summary>
public class EditWarehouseViewModel : EditItemViewModel
{
    private readonly IWarehouseService _warehouseService;

    public EditWarehouseViewModel(
        IWarehouseService warehouseService,
        IDialogService dialogService,
        ILogger<EditWarehouseViewModel> logger) : base(dialogService, logger)
    {
        _warehouseService = warehouseService;
    }

    public override string WindowTitle => "Редактирование склада";
    public override string LabelText => "Название склада:";
    public override string WatermarkText => "Введите название склада";
    public override string LoadingText => "Сохранение склада...";
    public override string SuccessMessage => $"Склад успешно переименован в '{ItemName}'";
    public override string ErrorMessageText => "Не удалось обновить склад";

    public void Initialize(Warehouse warehouse)
    {
        base.Initialize(warehouse.Id, warehouse.Name);
    }

    protected override async Task<bool> SaveItemAsync()
    {
        return await _warehouseService.UpdateWarehouseAsync(ItemId, ItemName);
    }
} 