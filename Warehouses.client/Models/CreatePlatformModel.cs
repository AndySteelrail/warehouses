using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Warehouses.client.Models;

/// <summary>
/// Модель для создания площадки
/// </summary>
public class CreatePlatformModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private ObservableCollection<CreatePicketModel> _pickets = new();
    
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
    
    public ObservableCollection<CreatePicketModel> Pickets
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}