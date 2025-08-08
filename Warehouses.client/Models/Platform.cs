using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Warehouses.client.Models;

/// <summary>
/// Модель площадки - временного объединения соседних пикетов
/// </summary>
public class Platform : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private DateTime _createdAt;
    private DateTime? _closedAt;
    private int _warehouseId;
    private List<Picket> _pickets = new();
    private Cargo? _currentCargo;
    
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
    
    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }
    }
    
    public DateTime? ClosedAt
    {
        get => _closedAt;
        set
        {
            if (_closedAt != value)
            {
                _closedAt = value;
                OnPropertyChanged(nameof(ClosedAt));
                OnPropertyChanged(nameof(IsActive));
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
    
    public List<Picket> Pickets
    {
        get => _pickets;
        set
        {
            if (_pickets != value)
            {
                _pickets = value;
                OnPropertyChanged(nameof(Pickets));
            }
        }
    }
    
    public Cargo? CurrentCargo
    {
        get => _currentCargo;
        set
        {
            if (_currentCargo != value)
            {
                _currentCargo = value;
                OnPropertyChanged(nameof(CurrentCargo));
            }
        }
    }
    
    public bool IsActive => ClosedAt == null;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 