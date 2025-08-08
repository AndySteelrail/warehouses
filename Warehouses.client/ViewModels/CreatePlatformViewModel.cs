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

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для создания площадки
/// </summary>
public class CreatePlatformViewModel : ViewModelBase
{
    private readonly IPlatformService _platformService;
    private readonly IPicketService _picketService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<CreatePlatformViewModel> _logger;

    private string _platformName = string.Empty;
    private ObservableCollection<PicketSelectionItem> _availablePickets = new();
    private DateTime _createdAt = DateTime.Now;
    private string _createdAtText;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();

    public CreatePlatformViewModel(
        IPlatformService platformService,
        IPicketService picketService,
        IDialogService dialogService,
        ILogger<CreatePlatformViewModel> logger)
    {
        _platformService = platformService;
        _picketService = picketService;
        _dialogService = dialogService;
        _logger = logger;

        CreateCommand = new AsyncRelayCommand(CreateAsync, CanCreate);
        CancelCommand = new RelayCommand(CancelAsync);
        LoadPicketsCommand = new AsyncRelayCommand(LoadPicketsAsync, CanLoadPickets);
        
        _createdAtText = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string PlatformName
    {
        get => _platformName;
        set
        {
            SetProperty(ref _platformName, value);
            CreateCommand.NotifyCanExecuteChanged();
            LoadPicketsCommand.NotifyCanExecuteChanged();
        }
    }

    public ObservableCollection<PicketSelectionItem> AvailablePickets
    {
        get => _availablePickets;
        set => SetProperty(ref _availablePickets, value);
    }
    
    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            SetProperty(ref _createdAt, value);
            LoadPicketsCommand.NotifyCanExecuteChanged();
        }
    }
    
    public string CreatedAtText
    {
        get => _createdAtText;
        set
        {
            if (SetProperty(ref _createdAtText, value))
            {
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    _createdAt = parsedDate;
                    LoadPicketsCommand.NotifyCanExecuteChanged();
                }
            }
        }
    }

    public string SelectedPicketsCount => $"Выбрано пикетов: {AvailablePickets.Count(p => p.IsSelected)}";

    public AsyncRelayCommand CreateCommand { get; }
    public RelayCommand CancelCommand { get; }
    public AsyncRelayCommand LoadPicketsCommand { get; }

    public int WarehouseId { get; set; }

    public event Action<bool>? WindowClosed;

    public async Task LoadPicketsAsync()
    {
        try
        {
            LoadingOverlay.LoadingText = "Загрузка пикетов...";
            LoadingOverlay.IsVisible = true;
            
            // Используем время создания площадки для получения актуального списка пикетов
            var pickets = await _picketService.GetPicketsByWarehouseAsync(WarehouseId, CreatedAt);
            var picketItems = pickets.Select(p => 
            {
                var item = new PicketSelectionItem { Picket = p, IsSelected = false };
                item.PropertyChanged += OnPicketSelectionChanged;
                return item;
            }).ToList();
            AvailablePickets = new ObservableCollection<PicketSelectionItem>(picketItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке пикетов для склада {WarehouseId} на время {CreatedAt}", WarehouseId, CreatedAt);
            await _dialogService.ShowMessageAsync("Ошибка", "Не удалось загрузить список пикетов");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
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
        try
        {
            LoadingOverlay.LoadingText = "Создание площадки...";
            LoadingOverlay.IsVisible = true;
            
            var selectedPickets = AvailablePickets.Where(p => p.IsSelected).Select(p => p.Picket).ToList();
            if (selectedPickets.Count == 0)
            {
                await _dialogService.ShowMessageAsync("Ошибка", "Необходимо выбрать хотя бы один пикет");
                return;
            }

            var picketIds = selectedPickets.Select(p => p.Id).ToList();
            var platform = await _platformService.CreatePlatformWithPicketsAsync(WarehouseId, PlatformName, picketIds, CreatedAt);

            if (platform != null)
            {
                _logger.LogInformation("Площадка успешно создана: PlatformId={PlatformId}", platform.Id);
                await _dialogService.ShowMessageAsync("Успех", $"Площадка '{platform.Name}' успешно создана");
                WindowClosed?.Invoke(true);
            }
            else
            {
                _logger.LogError("Создание площадки завершилось неудачно. Получен null результат");
                await _dialogService.ShowMessageAsync("Ошибка", "Не удалось создать площадку");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки");
            await _dialogService.ShowMessageAsync("Ошибка", ex.Message);
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private bool CanCreate()
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
        WindowClosed?.Invoke(false);
    }
} 