using System.ComponentModel;

namespace Warehouses.client.Models;

/// <summary>
/// Модель склада для отображения в пользовательском интерфейсе
/// </summary>
public class Warehouse : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 