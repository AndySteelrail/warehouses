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
/// ViewModel для создания пикета
/// </summary>
public partial class CreatePicketViewModel : NameViewModelBase
{
    private readonly IPicketService _picketService;
    private readonly IPlatformService _platformService;
    private readonly ILogger<CreatePicketViewModel> _logger;
    private readonly int _warehouseId;
    
    private Platform? _selectedPlatform;
    private bool _createNewPlatform;
    private string _newPlatformName = string.Empty;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CreatePicketViewModel(
        IPicketService picketService, 
        IPlatformService platformService,
        IDialogService dialogService,
        int warehouseId,
        ILogger<CreatePicketViewModel> logger) : base(dialogService)
    {
        _picketService = picketService;
        _platformService = platformService;
        _logger = logger;
        _warehouseId = warehouseId;
        
        AvailablePlatforms = new ObservableCollection<Platform>();
        
        _ = LoadAvailablePlatformsAsync();
    }
    
    public string PicketName
    {
        get => Name;
        set => Name = value;
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
    
    public override bool CanCreate => base.CanCreate && 
                           (SelectedPlatform != null || 
                            (CreateNewPlatform && !string.IsNullOrWhiteSpace(NewPlatformName)));
    
    // событие закрытия унаследовано из ModalViewModelBase
    
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
        if (!CanCreate || !ValidateName())
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
            
            var picket = await _picketService.CreatePicketAsync(platformId, warehouseId, GetCleanedName(), newPlatformName, GetCreatedAtUtc());
            
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
                
                await ShowSuccessAsync("Успех", message);
                
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
} 