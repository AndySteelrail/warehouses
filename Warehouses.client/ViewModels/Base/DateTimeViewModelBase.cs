using System;
using Warehouses.client.Services;
using Warehouses.client.Common;

namespace Warehouses.client.ViewModels.Base;

/// <summary>
/// Базовый класс для ViewModels с функционалом работы с датами
/// </summary>
public abstract class DateTimeViewModelBase : ModalViewModelBase
{
    private DateTime _createdAt = DateTime.Now;
    private string _createdAtText;
    
    protected DateTimeViewModelBase(IDialogService dialogService) : base(dialogService)
    {
        _createdAtText = _createdAt.ToString(Formatting.DateTimeHuman);
    }
    
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }
    
    public string CreatedAtText
    {
        get => _createdAtText;
        set
        {
            if (SetProperty(ref _createdAtText, value))
            {
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    CreatedAt = parsedDate;
                }
            }
        }
    }
    
    protected DateTime GetCreatedAtUtc()
    {
        return CreatedAt.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(CreatedAt, DateTimeKind.Local).ToUniversalTime()
            : CreatedAt.ToUniversalTime();
    }
}
