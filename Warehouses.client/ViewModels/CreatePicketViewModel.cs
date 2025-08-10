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
public partial class CreatePicketViewModel : FormViewModelBase
{
    private readonly IPicketService _picketService;
    private readonly IPlatformService _platformService;
    private readonly int _warehouseId;
    
    private Platform? _selectedPlatform;
    private bool _createNewPlatform;
    private string _newPlatformName = string.Empty;
    
    public CreatePicketViewModel(
        IPicketService picketService, 
        IPlatformService platformService,
        IDialogService dialogService,
        int warehouseId,
        ILogger<CreatePicketViewModel> logger) : base(logger, dialogService)
    {
        _picketService = picketService;
        _platformService = platformService;
        _warehouseId = warehouseId;
        
        AvailablePlatforms = new ObservableCollection<Platform>();
        
        // Подписываемся на изменения базовых свойств для обновления CanCreate
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(CreatedAt) || e.PropertyName == nameof(CreatedAtText))
            {
                OnPropertyChanged(nameof(CanCreate));
            }
        };
        
        _ = LoadAvailablePlatformsAsync();
    }
    
    public string PicketName
    {
        get => Name;
        set
        {
            Name = value;
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
    
    public bool CanCreate => !string.IsNullOrWhiteSpace(PicketName) && 
                           (SelectedPlatform != null || 
                            (CreateNewPlatform && !string.IsNullOrWhiteSpace(NewPlatformName)));
    
    
    private async Task LoadAvailablePlatformsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
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
            return true;
        }, "Загрузка площадок", "Ошибка при загрузке площадок");
    }
    
    [RelayCommand]
    private async Task Create()
    {
        if (!CanCreate)
        {
            SetError("Заполните все обязательные поля");
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            int? platformId = CreateNewPlatform ? null : SelectedPlatform?.Id;
            int? warehouseId = CreateNewPlatform ? _warehouseId : null;
            string? newPlatformName = CreateNewPlatform ? NewPlatformName.Trim() : null;
            
            var picket = await _picketService.CreatePicketAsync(platformId, warehouseId, GetCleanedName(), newPlatformName, GetCreatedAtUtc());
            
            if (picket == null)
            {
                throw new Exception("Не удалось создать пикет");
            }
            
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
            
            await ShowSuccessAsync(message);
            CloseWindow(true);
            return true;
        }, "Создание пикета", "Ошибка при создании пикета");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
} 