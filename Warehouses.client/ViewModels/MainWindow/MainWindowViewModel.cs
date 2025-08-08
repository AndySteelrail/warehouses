using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel главного окна приложения
/// </summary>
public partial class MainWindowViewModel : LoadingViewModelBase
{
    private readonly TreeDataService _treeDataService;
    private readonly MainWindowStateManager _stateManager;
    private readonly MainWindowOperations _operations;
    private readonly new ILogger _logger;
    private ObservableCollection<TreeNode> _warehousesTree = new();
    private ItemDetailsViewModel _itemDetailsViewModel;
    
    /// <summary>
    /// ViewModel для индикатора загрузки
    /// </summary>
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    /// <summary>
    /// ViewModel для сообщений об ошибках
    /// </summary>
    public ErrorMessageOverlayViewModel ErrorOverlay { get; } = new();

    /// <summary>
    /// Конструктор главного ViewModel
    /// </summary>
    public MainWindowViewModel(TreeDataService treeDataService, MainWindowStateManager stateManager, IDialogService dialogService, ILogger<MainWindowViewModel> logger)
        : base(logger)
    {
        _treeDataService = treeDataService;
        _stateManager = stateManager;
        _operations = new MainWindowOperations(dialogService, () => LoadDataCommand.ExecuteAsync(null));
        _logger = logger;
        Title = "Система управления складами";
        
        // Инициализируем ViewModel для деталей
        _itemDetailsViewModel = new ItemDetailsViewModel();
        
        // Загружаем данные при инициализации
        LoadDataCommand.ExecuteAsync(null);
    }
    
    public string Title { get; }
    
    public ObservableCollection<TreeNode> WarehousesTree
    {
        get => _warehousesTree;
        set => SetProperty(ref _warehousesTree, value);
    }
    

    
    public DateTime SelectedDate
    {
        get => _stateManager.SelectedDate;
        set
        {
            if (_stateManager.SelectedDate != value)
            {
                _stateManager.SelectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(SelectedDateText));
                _ = LoadDataCommand.ExecuteAsync(null);
            }
        }
    }
    
    public string SelectedDateText
    {
        get => _stateManager.SelectedDateText;
        set
        {
            if (_stateManager.SelectedDateText != value)
            {
                _stateManager.SelectedDateText = value;
                OnPropertyChanged(nameof(SelectedDateText));
                OnPropertyChanged(nameof(SelectedDate));
                _ = LoadDataCommand.ExecuteAsync(null);
            }
        }
    }
    
    public CargoType? SelectedCargoType
    {
        get => _stateManager.SelectedCargoType;
        set
        {
            if (_stateManager.SelectedCargoType != value)
            {
                _stateManager.SelectedCargoType = value;
                OnPropertyChanged(nameof(SelectedCargoType));
                // Обновляем данные при изменении типа груза
                _ = LoadDataCommand.ExecuteAsync(null);
            }
        }
    }
    
    public ObservableCollection<CargoType> CargoTypes
    {
        get => _stateManager.CargoTypes;
        set => _stateManager.CargoTypes = value;
    }
    
    public ItemDetailsViewModel ItemDetailsViewModel
    {
        get => _itemDetailsViewModel;
        set => SetProperty(ref _itemDetailsViewModel, value);
    }

    
    [RelayCommand]
    private async Task LoadData()
    {
        var ok = await ExecuteWithLoadingAsync(async () =>
        {
            ErrorOverlay.ClearError();

            if (!_stateManager.CargoTypes.Any())
            {
                await _stateManager.LoadCargoTypesAsync();
            }

            WarehousesTree = await _treeDataService.LoadWarehousesTreeAsync(
                _stateManager.SelectedDate,
                _stateManager.SelectedCargoType,
                async (node) => await _operations.EditAsync(node),
                async (node) => await _operations.DeleteAsync(node),
                async (node) => await _operations.CreatePicketAsync(node),
                async (node) => await _operations.CreatePlatformAsync(node),
                async (platformId) => await _operations.AddCargoAsync(new TreeNode { Id = platformId, NodeType = TreeNodeType.Platform })
            );
        }, "Загрузка данных", "Ошибка при загрузке данных");

        if (!ok)
        {
            ErrorOverlay.SetError(ErrorMessage);
        }
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadData();
    }

    
    [RelayCommand]
    private async Task CreateWarehouse()
    {
        await ExecuteActionAsync(() => _operations.CreateWarehouseAsync(), "Ошибка при создании склада");
    }
    
    [RelayCommand]
    private async Task<bool> CreatePlatform(TreeNode node)
    {
        return await ExecuteFuncAsync(() => _operations.CreatePlatformAsync(node), "Ошибка при создании площадки");
    }
    
    [RelayCommand]
    private async Task<bool> CreatePicket(TreeNode node)
    {
        return await ExecuteFuncAsync(() => _operations.CreatePicketAsync(node), "Ошибка при создании пикета");
    }
    
    [RelayCommand]
    private async Task<bool> Edit(TreeNode node)
    {
        if (node?.NodeType == TreeNodeType.CreateWarehouse)
        {
            return await ExecuteFuncAsync(() => _operations.CreateWarehouseAsync(), "Ошибка при создании склада");
        }
        return node != null && await ExecuteFuncAsync(() => _operations.EditAsync(node), "Ошибка при редактировании");
    }
    
    [RelayCommand]
    private async Task<bool> Delete(TreeNode node)
    {
        return await ExecuteFuncAsync(() => _operations.DeleteAsync(node), "Ошибка при удалении");
    }
    
    [RelayCommand]
    private async Task AddCargo(TreeNode node)
    {
        var result = await ExecuteFuncAsync(() => _operations.AddCargoAsync(node), "Ошибка при добавлении груза");
        if (result)
        {
            await LoadDataCommand.ExecuteAsync(null);
        }
    }

    private async Task ExecuteActionAsync(Func<Task> action, string errorPrefix)
    {
        try
        {
            ErrorOverlay.ClearError();
            await action();
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"{errorPrefix}: {ex.Message}");
        }
    }

    private async Task<bool> ExecuteFuncAsync(Func<Task<bool>> func, string errorPrefix)
    {
        try
        {
            ErrorOverlay.ClearError();
            return await func();
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"{errorPrefix}: {ex.Message}");
            return false;
        }
    }
}