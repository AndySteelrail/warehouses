using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;
using Warehouses.client.Models.DTO.Tree;
using Warehouses.client.Services;

namespace Warehouses.client.Services;

/// <summary>
/// Сервис для управления древовидной структурой данных
/// </summary>
public class TreeDataService
{
    private readonly IWarehouseService _warehouseService;

    public TreeDataService(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// Загружает древовидную структуру складов с площадками и пикетами
    /// </summary>
    public async Task<ObservableCollection<TreeNode>> LoadWarehousesTreeAsync(
        DateTime selectedDate,
        CargoType? selectedCargoType,
        Func<TreeNode, Task<bool>> editCallback,
        Func<TreeNode, Task<bool>> deleteCallback,
        Func<TreeNode, Task<bool>> createPicketCallback,
        Func<TreeNode, Task<bool>> createPlatformCallback,
        Func<int, Task<bool>> addCargoCallback)
    {
        var warehousesTree = new ObservableCollection<TreeNode>();

        try
        {
            // 1. Загружаем дерево складов с фильтрацией по времени и грузу
            int? cargoTypeId = null;
            if (selectedCargoType != null && selectedCargoType.Id > 0)
            {
                cargoTypeId = selectedCargoType.Id;
            }
            
            var tree = await _warehouseService.GetWarehousesTreeAsync(selectedDate, cargoTypeId);

            // 2. Создаем узлы для складов с площадками
            foreach (var warehouseDto in tree.Warehouses)
            {
                var warehouse = new Warehouse { Id = warehouseDto.Id, Name = warehouseDto.Name };
                var warehouseNode = CreateWarehouseNode(warehouse, editCallback, deleteCallback, createPicketCallback, createPlatformCallback);

                foreach (var platformDto in warehouseDto.Platforms)
                {
                    var platformNode = CreatePlatformNodeFromTree(platformDto, warehouseDto.Id, editCallback, deleteCallback, addCargoCallback);
                    warehouseNode.Children.Add(platformNode);
                }

                warehousesTree.Add(warehouseNode);
            }

            // 3. Добавляем кнопку создания нового склада в конец дерева
            var createWarehouseNode = new TreeNode
            {
                Id = -1,
                NodeType = TreeNodeType.CreateWarehouse,
                DisplayName = "➕ Создать склад"
            };
            warehousesTree.Add(createWarehouseNode);
        }
        catch (Exception ex)
        {
            // централизованная обработка: отдадим пустое дерево, логировать будет вызывающая сторона
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки иерархии складов: {ex.Message}");
            return new ObservableCollection<TreeNode>();
        }

        return warehousesTree;
    }

    private static TreeNode CreateWarehouseNode(
        Warehouse warehouse,
        Func<TreeNode, Task<bool>> editCallback,
        Func<TreeNode, Task<bool>> deleteCallback,
        Func<TreeNode, Task<bool>> createPicketCallback,
        Func<TreeNode, Task<bool>> createPlatformCallback)
    {
        return new TreeNode
        {
            Id = warehouse.Id,
            NodeType = TreeNodeType.Warehouse,
            DisplayName = $"🏢 {warehouse.Name}",
            Data = warehouse,
            CanEdit = true,
            CanDelete = true,
            CanCreatePicket = true,
            CanCreatePlatform = true,
            EditCallback = editCallback,
            DeleteCallback = deleteCallback,
            CreatePicketCallback = createPicketCallback,
            CreatePlatformCallback = createPlatformCallback
        };
    }



    private static TreeNode CreatePicketNode(
        Picket picket,
        Func<TreeNode, Task<bool>> editCallback,
        Func<TreeNode, Task<bool>> deleteCallback)
    {
        return new TreeNode
        {
            Id = picket.Id,
            ParentId = 0,
            NodeType = TreeNodeType.Picket,
            DisplayName = $"📍 {picket.Name}",
            CanEdit = true,
            CanDelete = true,
            EditCallback = editCallback,
            DeleteCallback = deleteCallback,
            Data = picket
        };
    }

    private static TreeNode CreatePlatformNodeFromTree(
        PlatformTreeDTO platformDto,
        int warehouseId,
        Func<TreeNode, Task<bool>> editCallback,
        Func<TreeNode, Task<bool>> deleteCallback,
        Func<int, Task<bool>> addCargoCallback)
    {
        var platformNode = new TreeNode
        {
            Id = platformDto.Id,
            ParentId = warehouseId,
            NodeType = TreeNodeType.Platform,
            DisplayName = $"📦 {platformDto.Name}",
            CanAddCargo = true,
            CanEdit = true,
            CanDelete = true,
            AddCargoCallback = addCargoCallback,
            EditCallback = editCallback,
            DeleteCallback = deleteCallback,
            Data = new Platform 
            { 
                Id = platformDto.Id, 
                Name = platformDto.Name,
                WarehouseId = warehouseId
            }
        };

        // Добавляем информацию о грузе
        platformNode.CargoInfo = $"{platformDto.CargoAmount} т. ({platformDto.CargoType})";
        platformNode.CargoType = platformDto.CargoType;
        platformNode.CargoAmount = platformDto.CargoAmount;

        // Добавляем пикеты из DTO
        foreach (var picketDto in platformDto.Pickets)
        {
            var picketNode = new TreeNode
            {
                Id = picketDto.Id,
                ParentId = platformDto.Id,
                NodeType = TreeNodeType.Picket,
                DisplayName = $"📍 {picketDto.Name}",
                CanEdit = true,
                CanDelete = true,
                EditCallback = editCallback,
                DeleteCallback = deleteCallback,
                Data = new Picket { Id = picketDto.Id, Name = picketDto.Name }
            };
            platformNode.Children.Add(picketNode);
        }

        return platformNode;
    }
}
