using Microsoft.Extensions.Logging;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels.Base;

public abstract class CloseItemViewModelBase : FormViewModelBase
{
    protected CloseItemViewModelBase(IDialogService dialogService, ILogger logger) : base(logger, dialogService)
    {
    }

    public abstract string WindowTitle { get; }
    public abstract string ConfirmText { get; }
    public abstract string TargetName { get; }
}