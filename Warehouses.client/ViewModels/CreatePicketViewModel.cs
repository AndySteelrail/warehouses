using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для создания пикета
/// </summary>
public partial class CreatePicketViewModel : ViewModelBase
{
    private readonly IPicketService _picketService;
    private readonly IPlatformService _platformService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<CreatePicketViewModel> _logger;
    private readonly int _warehouseId;
    
    private string _picketName = string.Empty;
    private Platform? _selectedPlatform;
    private bool _createNewPlatform;
    private string _newPlatformName = string.Empty;
    private DateTime _createdAt = DateTime.Now;
    private string _createdAtText;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CreatePicketViewModel(
        IPicketService picketService, 
        IPlatformService platformService,
        IDialogService dialogService,
        int warehouseId,
        ILogger<CreatePicketViewModel> logger)
    {
        _picketService = picketService;
        _platformService = platformService;
        _dialogService = dialogService;
        _logger = logger;
        _warehouseId = warehouseId;
        
        AvailablePlatforms = new ObservableCollection<Platform>();
        
        _createdAtText = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
        
        _ = LoadAvailablePlatformsAsync();
    }
    
    public string PicketName
    {
        get => _picketName;
        set
        {
            SetProperty(ref _picketName, value);
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public ObservableCollection<Platform> AvailablePlatforms { get; }
    
    public Platform? SelectedPlatform
    {
        get => _selectedPlatform;
        set
        {
            SetProperty(ref _selectedPlatform, value);
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public bool CreateNewPlatform
    {
        get => _createNewPlatform;
        set
        {
            SetProperty(ref _createNewPlatform, value);
            if (value)
            {
                SelectedPlatform = null;
                NewPlatformName = PicketName;
            }
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public string NewPlatformName
    {
        get => _newPlatformName;
        set
        {
            SetProperty(ref _newPlatformName, value);
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
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
                }
            }
        }
    }
    
    public bool CanCreate => !string.IsNullOrWhiteSpace(PicketName) && 
                           (SelectedPlatform != null || 
                            (CreateNewPlatform && !string.IsNullOrWhiteSpace(NewPlatformName)));
    
    public event Action<bool>? WindowClosed;
    
    private async Task LoadAvailablePlatformsAsync()
    {
        try
        {
            LoadingOverlay.LoadingText = "Загрузка площадок...";
            LoadingOverlay.IsVisible = true;
            ClearError();

            var platforms = await _platformService.GetPlatformsByWarehouseAsync(_warehouseId);
            
            AvailablePlatforms.Clear();
            foreach (var platform in platforms)
            {
                AvailablePlatforms.Add(platform);
            }

            // Если площадок нет, предлагаем создать новую
            if (!AvailablePlatforms.Any())
            {
                CreateNewPlatform = true;
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при загрузке площадок: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
    
    [RelayCommand]
    private async Task Create()
    {
        if (!CanCreate)
        {
            _logger.LogWarning("Попытка создать пикет без заполнения обязательных полей");
            SetError("Заполните все обязательные поля");
            return;
        }

        try
        {
            LoadingOverlay.LoadingText = "Создание пикета...";
            LoadingOverlay.IsVisible = true;
            ClearError();

            // Универсальный вызов создания пикета
            int? platformId = CreateNewPlatform ? null : SelectedPlatform?.Id;
            int? warehouseId = CreateNewPlatform ? _warehouseId : null;
            string? newPlatformName = CreateNewPlatform ? NewPlatformName.Trim() : null;
            
            var picket = await _picketService.CreatePicketAsync(platformId, warehouseId, PicketName.Trim(), newPlatformName, CreatedAt);
            
            if (picket != null)
            {
                _logger.LogInformation("Пикет создан успешно. PicketId={PicketId}", picket.Id);
                
                var message = $"Пикет '{picket.Name}' успешно создан";
                if (CreateNewPlatform)
                {
                    message += $" на новой площадке '{NewPlatformName.Trim()}'";
                }
                else if (SelectedPlatform != null)
                {
                    message += $" на площадке '{SelectedPlatform.Name}'";
                }
                
                if (CreatedAt != DateTime.Now)
                {
                    message += $" на время {CreatedAt:yyyy-MM-dd HH:mm:ss}";
                }
                
                await _dialogService.ShowMessageAsync("Успех", message);
                
                CloseWindow(true);
                return;
            }
            else
            {
                _logger.LogError("Создание пикета завершилось неудачно. Получен null результат");
                SetError("Не удалось создать пикет");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пикета");
            SetError($"Ошибка при создании пикета: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
    
    private void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }
} 