using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel главного окна приложения
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TreeDataService _treeDataService;
    private readonly MainWindowStateManager _stateManager;
    private readonly MainWindowOperations _operations;
    private ObservableCollection<TreeNode> _warehousesTree = new();
    private TreeNode? _selectedTreeNode;
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
    public MainWindowViewModel(TreeDataService treeDataService, MainWindowStateManager stateManager, IDialogService dialogService)
    {
        _treeDataService = treeDataService;
        _stateManager = stateManager;
        _operations = new MainWindowOperations(dialogService, () => LoadDataCommand.ExecuteAsync(null));
        Title = "Система управления складами";
        
        // Инициализируем ViewModel для деталей
        _itemDetailsViewModel = new ItemDetailsViewModel();
        

        
        // Загружаем данные при инициализации
        LoadDataCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Заголовок окна
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Древовидная структура складов
    /// </summary>
    public ObservableCollection<TreeNode> WarehousesTree
    {
        get => _warehousesTree;
        set => SetProperty(ref _warehousesTree, value);
    }

    /// <summary>
    /// Выбранный узел в дереве
    /// </summary>
    public TreeNode? SelectedTreeNode
    {
        get => _selectedTreeNode;
        set
        {
            if (SetProperty(ref _selectedTreeNode, value))
            {
                // Обновляем детали при изменении выбранного узла
                _itemDetailsViewModel.SelectedNode = value;
            }
        }
    }

    /// <summary>
    /// Выбранная дата для фильтрации
    /// </summary>
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
                // Обновляем данные при изменении даты
                _ = LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Текстовое представление выбранной даты
    /// </summary>
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
                // Обновляем данные при изменении даты
                _ = LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Выбранный тип груза для фильтрации
    /// </summary>
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

    /// <summary>
    /// Список доступных типов грузов
    /// </summary>
    public ObservableCollection<CargoType> CargoTypes
    {
        get => _stateManager.CargoTypes;
        set => _stateManager.CargoTypes = value;
    }

    /// <summary>
    /// Детали выбранного элемента
    /// </summary>
    public ItemDetailsViewModel ItemDetailsViewModel
    {
        get => _itemDetailsViewModel;
        set => SetProperty(ref _itemDetailsViewModel, value);
    }



    /// <summary>
    /// Команда загрузки данных
    /// </summary>
    [RelayCommand]
    private async Task LoadData()
    {
        try
        {
            LoadingOverlay.LoadingText = "Загрузка данных...";
            LoadingOverlay.IsVisible = true;
            ErrorOverlay.ClearError();

            // Загружаем типы грузов только если их еще нет
            if (!_stateManager.CargoTypes.Any())
            {
                await _stateManager.LoadCargoTypesAsync();
            }

            // Загружаем древовидную структуру
            WarehousesTree = await _treeDataService.LoadWarehousesTreeAsync(
                _stateManager.SelectedDate,
                _stateManager.SelectedCargoType,
                async (node) => await _operations.EditAsync(node),
                async (node) => await _operations.DeleteAsync(node),
                async (node) => await _operations.CreatePicketAsync(node),
                async (node) => await _operations.CreatePlatformAsync(node),
                async (platformId) => await _operations.AddCargoAsync(new TreeNode { Id = platformId, NodeType = TreeNodeType.Platform })
            );
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при загрузке данных: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"LoadData error: {ex}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadData();
    }



    /// <summary>
    /// Команда создания склада
    /// </summary>
    [RelayCommand]
    private async Task CreateWarehouse()
    {
        try
        {
            await _operations.CreateWarehouseAsync();
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при создании склада: {ex.Message}");
        }
    }

    /// <summary>
    /// Команда создания площадки
    /// </summary>
    [RelayCommand]
    private async Task<bool> CreatePlatform(TreeNode node)
    {
        try
        {
            return await _operations.CreatePlatformAsync(node);
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при создании площадки: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Команда создания пикета
    /// </summary>
    [RelayCommand]
    private async Task<bool> CreatePicket(TreeNode node)
    {
        try
        {
            return await _operations.CreatePicketAsync(node);
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при создании пикета: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Команда редактирования элемента
    /// </summary>
    [RelayCommand]
    private async Task<bool> Edit(TreeNode node)
    {
        try
        {
            if (node?.NodeType == TreeNodeType.CreateWarehouse)
            {
                return await _operations.CreateWarehouseAsync();
            }
            return node != null && await _operations.EditAsync(node);
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при редактировании: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Команда удаления элемента
    /// </summary>
    [RelayCommand]
    private async Task<bool> Delete(TreeNode node)
    {
        try
        {
            return await _operations.DeleteAsync(node);
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при удалении: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Команда добавления груза
    /// </summary>
    [RelayCommand]
    private async Task AddCargo(TreeNode node)
    {
        try
        {
            var result = await _operations.AddCargoAsync(node);
            if (result)
            {
                await LoadDataCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            ErrorOverlay.SetError($"Ошибка при добавлении груза: {ex.Message}");
        }
    }






}