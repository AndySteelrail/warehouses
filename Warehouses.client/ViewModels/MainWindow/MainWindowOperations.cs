using System;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// Операции CRUD для главного окна
/// </summary>
public class MainWindowOperations
{
    private readonly IDialogService _dialogService;
    private readonly Action _refreshDataCallback;

    public MainWindowOperations(IDialogService dialogService, Action refreshDataCallback)
    {
        _dialogService = dialogService;
        _refreshDataCallback = refreshDataCallback;
    }
    
    public async Task<bool> CreateWarehouseAsync()
    {
        try
        {
            var result = await _dialogService.ShowCreateWarehouseDialogAsync();
            if (result)
            {
                _refreshDataCallback();
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при создании склада: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> CreatePlatformAsync(TreeNode node)
    {
        try
        {
            if (node?.NodeType == TreeNodeType.Warehouse)
            {
                var warehouse = node.Data as Warehouse;
                if (warehouse != null)
                {
                    var result = await _dialogService.ShowCreatePlatformDialogAsync(warehouse.Id);
                    if (result)
                    {
                        _refreshDataCallback();
                    }
                    return result;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при создании площадки: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> CreatePicketAsync(TreeNode node)
    {
        try
        {
            if (node?.NodeType == TreeNodeType.Warehouse)
            {
                var warehouse = node.Data as Warehouse;
                if (warehouse != null)
                {
                    var result = await _dialogService.ShowCreatePicketDialogAsync(warehouse.Id);
                    if (result)
                    {
                        _refreshDataCallback();
                    }
                    return result;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при создании пикета: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> EditAsync(TreeNode node)
    {
        try
        {
            if (node == null) return false;

            bool result = false;
            switch (node.NodeType)
            {
                case TreeNodeType.Warehouse:
                    var warehouse = node.Data as Warehouse;
                    if (warehouse != null)
                    {
                        result = await _dialogService.ShowEditWarehouseDialogAsync(warehouse);
                    }
                    break;

                case TreeNodeType.Platform:
                    var platform = node.Data as Platform;
                    if (platform != null)
                    {
                        result = await _dialogService.ShowEditPlatformDialogAsync(platform);
                    }
                    break;

                case TreeNodeType.Picket:
                    var picket = node.Data as Picket;
                    if (picket != null)
                    {
                        result = await _dialogService.ShowEditPicketDialogAsync(picket);
                    }
                    break;
            }

            if (result)
            {
                _refreshDataCallback();
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при редактировании: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> DeleteAsync(TreeNode node)
    {
        try
        {
            if (node == null) return false;

            bool result = false;
            switch (node.NodeType)
            {
                case TreeNodeType.Warehouse:
                    var warehouse = node.Data as Warehouse;
                    if (warehouse != null)
                    {
                        result = await _dialogService.ShowCloseWarehouseDialogAsync(warehouse);
                    }
                    break;

                case TreeNodeType.Platform:
                    await _dialogService.ShowMessageAsync("Информация", 
                        "Площадки нельзя удалить напрямую. Они автоматически закрываются при перераспределении пикетов между площадками или при закрытии склада.");
                    break;

                case TreeNodeType.Picket:
                    var picket = node.Data as Picket;
                    if (picket != null)
                    {
                        result = await _dialogService.ShowClosePicketDialogAsync(picket);
                    }
                    break;
            }

            if (result)
            {
                _refreshDataCallback();
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при удалении: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> AddCargoAsync(TreeNode node)
    {
        try
        {
            int platformId = 0;
            
            if (node?.NodeType == TreeNodeType.Platform)
            {
                var platform = node.Data as Platform;
                if (platform != null)
                {
                    platformId = platform.Id;
                }
                else
                {
                    platformId = node.Id;
                }
            }
            
            if (platformId > 0)
            {
                var result = await _dialogService.ShowAddCargoDialogAsync(platformId);
                if (result)
                {
                    _refreshDataCallback();
                }
                return result;
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при добавлении груза: {ex.Message}", ex);
        }
    }
}
