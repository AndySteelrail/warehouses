using Warehouses.client.Services;

namespace Warehouses.client.ViewModels.Base;

public abstract class CloseItemViewModelBase : DateTimeViewModelBase
{
    protected CloseItemViewModelBase(IDialogService dialogService) : base(dialogService)
    {
    }

    public abstract string WindowTitle { get; }
    public abstract string ConfirmText { get; }
    public abstract string TargetName { get; }
}


