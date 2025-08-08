using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для создания площадки
/// </summary>
public class CreatePlatformViewModel : NameViewModelBase
{
    private readonly IPlatformService _platformService;
    private readonly IPicketService _picketService;
    private readonly ILogger<CreatePlatformViewModel> _logger;

    private ObservableCollection<PicketSelectionItem> _availablePickets = new();
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();

    public CreatePlatformViewModel(
        IPlatformService platformService,
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<CreatePlatformViewModel> logger) : base(dialogService)
    {
        _platformService = platformService;
        _picketService = picketService;
        _logger = logger;

        CreateCommand = new AsyncRelayCommand(CreateAsync, CanCreatePlatform);
        CancelCommand = new RelayCommand(CancelAsync);
        LoadPicketsCommand = new AsyncRelayCommand(LoadPicketsAsync, CanLoadPickets);
    }

    public string PlatformName
    {
        get => Name;
        set
        {
            Name = value;
            CreateCommand.NotifyCanExecuteChanged();
            LoadPicketsCommand.NotifyCanExecuteChanged();
        }
    }

    public ObservableCollection<PicketSelectionItem> AvailablePickets
    {
        get => _availablePickets;
        set => SetProperty(ref _availablePickets, value);
    }
    
    public new DateTime CreatedAt
    {
        get => base.CreatedAt;
        set
        {
            base.CreatedAt = value;
            LoadPicketsCommand.NotifyCanExecuteChanged();
        }
    }
    
    public new string CreatedAtText
    {
        get => base.CreatedAtText;
        set
        {
            base.CreatedAtText = value;
            LoadPicketsCommand.NotifyCanExecuteChanged();
        }
    }

    public string SelectedPicketsCount => $"Выбрано пикетов: {AvailablePickets.Count(p => p.IsSelected)}";

    public AsyncRelayCommand CreateCommand { get; }
    public RelayCommand CancelCommand { get; }
    public AsyncRelayCommand LoadPicketsCommand { get; }

    public int WarehouseId { get; set; }


    public async Task LoadPicketsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var pickets = await _picketService.GetPicketsByWarehouseAsync(WarehouseId, CreatedAt);
            var picketItems = pickets.Select(p =>
            {
                var item = new PicketSelectionItem { Picket = p, IsSelected = false };
                item.PropertyChanged += OnPicketSelectionChanged;
                return item;
            }).ToList();
            AvailablePickets = new ObservableCollection<PicketSelectionItem>(picketItems);
        }, "Загрузка пикетов", "Не удалось загрузить список пикетов");
    }

    private void OnPicketSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PicketSelectionItem.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedPicketsCount));
            CreateCommand.NotifyCanExecuteChanged();
        }
    }

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        
        if (propertyName == nameof(PlatformName) || propertyName == nameof(CreatedAt))
        {
            CreateCommand.NotifyCanExecuteChanged();
            LoadPicketsCommand.NotifyCanExecuteChanged();
            
            // Сбрасываем список пикетов при изменении имени или времени
            if (propertyName == nameof(PlatformName) || propertyName == nameof(CreatedAt))
            {
                AvailablePickets.Clear();
            }
        }
    }

    private async Task CreateAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var selectedPickets = AvailablePickets.Where(p => p.IsSelected).Select(p => p.Picket).ToList();
            if (selectedPickets.Count == 0)
            {
                await ShowErrorAsync("Необходимо выбрать хотя бы один пикет");
                return;
            }

            var picketIds = selectedPickets.Select(p => p.Id).ToList();
            var platform = await _platformService.CreatePlatformWithPicketsAsync(WarehouseId, PlatformName.Trim(), picketIds, GetCreatedAtUtc());

            if (platform == null)
            {
                throw new Exception("Не удалось создать площадку");
            }

            _logger.LogInformation("Площадка успешно создана: PlatformId={PlatformId}", platform.Id);
            await ShowSuccessAsync($"Площадка '{platform.Name}' успешно создана");
            CloseWindow(true);
        }, "Создание площадки", "Ошибка при создании площадки");
    }

    private bool CanCreatePlatform()
    {
        var selectedCount = AvailablePickets.Count(p => p.IsSelected);
        return !string.IsNullOrWhiteSpace(PlatformName) && selectedCount > 0 && !LoadingOverlay.IsVisible;
    }

    private bool CanLoadPickets()
    {
        return !string.IsNullOrWhiteSpace(PlatformName) && !LoadingOverlay.IsVisible;
    }

    private void CancelAsync()
    {
        CloseWindow(false);
    }

    private async Task<bool> ExecuteWithLoadingAsync(Func<Task> operation, string loadingText, string errorMessage)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            await ShowErrorAsync($"{errorMessage}: {ex.Message}");
            return false;
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
} 