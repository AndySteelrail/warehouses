using System;
using System.ComponentModel;

namespace Warehouses.client.Models;

/// <summary>
/// Модель груза на площадке
/// </summary>
public class Cargo : INotifyPropertyChanged
{
    private int _id;
    private decimal _remainder;
    private decimal _coming;
    private decimal _consumption;
    private DateTime _recordedAt;
    private int _platformId;
    private string _goodType = string.Empty;

    
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
    
    public decimal Remainder
    {
        get => _remainder;
        set
        {
            if (_remainder != value)
            {
                _remainder = value;
                OnPropertyChanged(nameof(Remainder));
            }
        }
    }
    
    public decimal Coming
    {
        get => _coming;
        set
        {
            if (_coming != value)
            {
                _coming = value;
                OnPropertyChanged(nameof(Coming));
            }
        }
    }
    
    public decimal Consumption
    {
        get => _consumption;
        set
        {
            if (_consumption != value)
            {
                _consumption = value;
                OnPropertyChanged(nameof(Consumption));
            }
        }
    }
    
    public DateTime RecordedAt
    {
        get => _recordedAt;
        set
        {
            if (_recordedAt != value)
            {
                _recordedAt = value;
                OnPropertyChanged(nameof(RecordedAt));
            }
        }
    }
    
    public int PlatformId
    {
        get => _platformId;
        set
        {
            if (_platformId != value)
            {
                _platformId = value;
                OnPropertyChanged(nameof(PlatformId));
            }
        }
    }
    
    public string GoodType
    {
        get => _goodType;
        set
        {
            if (_goodType != value)
            {
                _goodType = value;
                OnPropertyChanged(nameof(GoodType));
            }
        }
    }



    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 