using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;
using Warehouses.client.Common;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для добавления груза
/// </summary>
public partial class AddCargoViewModel : FormViewModelBase
{
    private readonly IReferenceService _referenceService;
    private readonly ICargoService _cargoService;
    private readonly new IDialogService _dialogService;
    private readonly int _platformId;
    private ObservableCollection<OperationType> _operationTypes = new();
    private ObservableCollection<CargoType> _cargoTypes = new();

    private OperationType? _selectedOperationType;
    private CargoType? _selectedCargoType;

    private string _quantity = string.Empty;
    private DateTime _operationDate = DateTime.Today;
    private string _operationDateText;
           


           
    public AddCargoViewModel(IReferenceService referenceService, ICargoService cargoService, IDialogService dialogService, ILogger<AddCargoViewModel> logger, int platformId) : base(logger, dialogService)
    {
        _referenceService = referenceService;
        _cargoService = cargoService;
        _dialogService = dialogService;
        _platformId = platformId;
        
        InitializeOperationTypes();
        
        _operationDateText = _operationDate.ToString(Formatting.DateTimeHuman);
        
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
       get => CreatedAt;
       set
       {
           CreatedAt = value;
           SetProperty(ref _operationDate, value, nameof(OperationDate));
       }
   }
       
   public string OperationDateText
   {
       get => CreatedAtText;
       set
       {
           CreatedAtText = value;
           SetProperty(ref _operationDateText, value, nameof(OperationDateText));
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
        await ExecuteWithOverlayAsync(async () =>
        {
            var cargoTypes = await _referenceService.GetCargoTypesAsync();
            CargoTypes.Clear();
            foreach (var cargoType in cargoTypes)
            {
                CargoTypes.Add(cargoType);
            }
        }, "Загрузка справочников", "Ошибка при загрузке справочников");
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

        await ExecuteWithOverlayAsync(async () =>
        {
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
                GetCreatedAtUtc()
            );
            
            var operationType = SelectedOperationType?.Name ?? "операция";
            var cargoType = SelectedCargoType?.Name ?? "груз";
            var message = $"Успешно добавлен {operationType} груза '{cargoType}' в количестве {quantityValue:N3} тонн";
            await ShowSuccessAsync(message);
            
            CloseWindow(true);
        }, "Добавление груза", "Ошибка при добавлении груза");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
    
    private async Task<bool> ExecuteWithOverlayAsync(Func<Task> action, string loadingText, string errorPrefix)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            await action();
            return true;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"{errorPrefix}: {ex.Message}");
            return false;
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
} 