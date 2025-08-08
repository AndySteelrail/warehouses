using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Warehouses.client.Models;

/// <summary>
/// Элемент выбора пикета
/// </summary>
public class PicketSelectionItem : INotifyPropertyChanged
{
    private Picket _picket = new();
    private bool _isSelected;

    public Picket Picket
    {
        get => _picket;
        set => SetProperty(ref _picket, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string DisplayName => $"{_picket.Name} {(IsSelected ? "✓" : "")}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
} 