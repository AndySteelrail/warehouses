using System.ComponentModel;

namespace Warehouses.client.Models;

/// <summary>
/// Модель пикета - минимальной единицы склада
/// </summary>
public class Picket : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private int _warehouseId;
    
    public int Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
    }
    
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    
    public int WarehouseId
    {
        get => _warehouseId;
        set
        {
            if (_warehouseId != value)
            {
                _warehouseId = value;
                OnPropertyChanged(nameof(WarehouseId));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 