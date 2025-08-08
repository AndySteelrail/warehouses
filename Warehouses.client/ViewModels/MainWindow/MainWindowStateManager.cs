using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.Common;

namespace Warehouses.client.ViewModels;

/// <summary>
/// Менеджер состояния главного окна
/// </summary>
public class MainWindowStateManager
{
    private readonly IReferenceService _referenceService;
    private DateTime _selectedDate = DateTime.Now;
    private CargoType? _selectedCargoType;
    private ObservableCollection<CargoType> _cargoTypes = new();
    private string _selectedDateText;

    public MainWindowStateManager(IReferenceService referenceService)
    {
        _referenceService = referenceService;
        _selectedDateText = _selectedDate.ToString(Formatting.DateTimeHuman);
    }
    
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate != value)
            {
                _selectedDate = value;
                _selectedDateText = _selectedDate.ToString(Formatting.DateTimeHuman);
            }
        }
    }
    
    public string SelectedDateText
    {
        get => _selectedDateText;
        set
        {
            if (_selectedDateText != value)
            {
                _selectedDateText = value;
                if (DateTime.TryParse(_selectedDateText, out var parsedDate))
                {
                    _selectedDate = parsedDate;
                }
            }
        }
    }
    
    public CargoType? SelectedCargoType
    {
        get => _selectedCargoType;
        set
        {
            if (_selectedCargoType != value)
            {
                _selectedCargoType = value;
            }
        }
    }
    
    public ObservableCollection<CargoType> CargoTypes
    {
        get => _cargoTypes;
        set => _cargoTypes = value;
    }

    
    public async Task LoadCargoTypesAsync()
    {
        // Загружаем типы грузов (ошибку отдаст ApiService)
        var cargoTypes = await _referenceService.GetCargoTypesAsync();
        CargoTypes.Clear();
        
        var allCargoType = new CargoType { Id = 0, Name = "Все грузы" };
        CargoTypes.Add(allCargoType);
        
        foreach (var cargoType in cargoTypes)
        {
            CargoTypes.Add(cargoType);
        }
        
        _selectedCargoType = allCargoType;
    }
    
    public void UpdateDateText()
    {
        _selectedDateText = _selectedDate.ToString(Formatting.DateTimeHuman);
    }

}
