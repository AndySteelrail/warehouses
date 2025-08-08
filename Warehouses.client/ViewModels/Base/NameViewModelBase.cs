using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels.Base;

/// <summary>
/// Базовый класс для ViewModels с функционалом работы с именами
/// </summary>
public abstract class NameViewModelBase : DateTimeViewModelBase
{
    private string _name = string.Empty;
    
    protected NameViewModelBase(IDialogService dialogService) : base(dialogService)
    {
    }
    
    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public virtual bool CanCreate => !string.IsNullOrWhiteSpace(Name);
    
    protected string GetCleanedName()
    {
        return Name.Trim();
    }
    
    protected bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            SetError("Введите название");
            return false;
        }
        
        ClearError();
        return true;
    }
}
