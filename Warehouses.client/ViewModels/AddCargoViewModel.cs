using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для добавления груза
/// </summary>
public partial class AddCargoViewModel : ViewModelBase
{
    private readonly IReferenceService _referenceService;
    private readonly ICargoService _cargoService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<AddCargoViewModel> _logger;
    private readonly int _platformId;
    private ObservableCollection<OperationType> _operationTypes = new();
    private ObservableCollection<CargoType> _cargoTypes = new();

    private OperationType? _selectedOperationType;
    private CargoType? _selectedCargoType;

    private string _quantity = string.Empty;
    private DateTime _operationDate = DateTime.Today;
    private string _operationDateText;
           

    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
           
    public AddCargoViewModel(IReferenceService referenceService, ICargoService cargoService, IDialogService dialogService, ILogger<AddCargoViewModel> logger, int platformId)
    {
        _referenceService = referenceService;
        _cargoService = cargoService;
        _dialogService = dialogService;
        _logger = logger;
        _platformId = platformId;
        
        InitializeOperationTypes();
        
        _operationDateText = _operationDate.ToString("yyyy-MM-dd HH:mm:ss");
        
        LoadReferenceData();
    }
    
    public ObservableCollection<OperationType> OperationTypes
    {
        get => _operationTypes;
        set => SetProperty(ref _operationTypes, value);
    }
    
    public ObservableCollection<CargoType> CargoTypes
    {
        get => _cargoTypes;
        set => SetProperty(ref _cargoTypes, value);
    }
    
    public OperationType? SelectedOperationType
    {
        get => _selectedOperationType;
        set
        {
            SetProperty(ref _selectedOperationType, value);
            OnPropertyChanged(nameof(CanAdd));
        }
    }
    
    public CargoType? SelectedCargoType
    {
        get => _selectedCargoType;
        set
        {
            SetProperty(ref _selectedCargoType, value);
            OnPropertyChanged(nameof(CanAdd));
        }
    }

    
    public string Quantity
    {
        get => _quantity;
        set
        {
            SetProperty(ref _quantity, value);
            OnPropertyChanged(nameof(CanAdd));
        }
    }

   public DateTime OperationDate
   {
       get => _operationDate;
       set => SetProperty(ref _operationDate, value);
   }
       
   public string OperationDateText
   {
       get => _operationDateText;
       set
       {
           if (SetProperty(ref _operationDateText, value))
           {
               if (DateTime.TryParse(value, out var parsedDate))
               {
                   _operationDate = parsedDate;
               }
           }
       }
   }
           
   public bool CanAdd => SelectedOperationType != null && 
                         SelectedCargoType != null && 
                         !string.IsNullOrWhiteSpace(Quantity) &&
                         decimal.TryParse(Quantity, out _);
    
    private void InitializeOperationTypes()
    {
        OperationTypes.Clear();
        OperationTypes.Add(new OperationType { Id = 1, Name = "Приход" });
        OperationTypes.Add(new OperationType { Id = 2, Name = "Расход" });
        SelectedOperationType = OperationTypes.FirstOrDefault();
    }
    
    private async void LoadReferenceData()
    {
        try
        {
            LoadingOverlay.LoadingText = "Загрузка справочников...";
            LoadingOverlay.IsVisible = true;
            ClearError();
            
            var cargoTypes = await _referenceService.GetCargoTypesAsync();
            CargoTypes.Clear();
            foreach (var cargoType in cargoTypes)
            {
                CargoTypes.Add(cargoType);
            }
            
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при загрузке справочников: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
    
    [RelayCommand]
    private async Task Add()
    {
        if (!CanAdd)
        {
            SetError("Заполните все обязательные поля");
            return;
        }

        if (!decimal.TryParse(Quantity, out var quantityValue))
        {
            SetError("Введите корректное количество");
            return;
        }

        if (quantityValue <= 0)
        {
            SetError("Количество должно быть больше нуля");
            return;
        }



        try
        {
            LoadingOverlay.LoadingText = "Добавление груза...";
            LoadingOverlay.IsVisible = true;
            ClearError();
            
            decimal? coming = null;
            decimal? consumption = null;

            if (SelectedOperationType?.Id == 1) // Приход
            {
                coming = quantityValue;
            }
            else if (SelectedOperationType?.Id == 2) // Расход
            {
                consumption = quantityValue;
            }

            
            await _cargoService.AddCargoOperationAsync(
                _platformId,
                SelectedCargoType!.Id,
                coming,
                consumption,
                OperationDate
            );
            
            _logger.LogInformation("Груз успешно добавлен");
            
            var operationType = SelectedOperationType?.Name ?? "операция";
            var cargoType = SelectedCargoType?.Name ?? "груз";
            var message = $"Успешно добавлен {operationType} груза '{cargoType}' в количестве {quantityValue:N3} тонн";
            await _dialogService.ShowMessageAsync("Успех", message);
            
            CloseWindow(true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении груза: {Message}", ex.Message);
            await _dialogService.ShowMessageAsync("Ошибка", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при добавлении груза: {Message}", ex.Message);
            await _dialogService.ShowMessageAsync("Ошибка", $"Неожиданная ошибка: {ex.Message}");
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
    
    public event Action<bool>? WindowClosed;
    
    private void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }
} 