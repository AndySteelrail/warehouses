using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;
using Warehouses.client.Common;

namespace Warehouses.client.ViewModels.Base;

/// <summary>
/// Базовый класс для ViewModels форм с полями Name, CreatedAt и LoadingOverlay
/// </summary>
public abstract class FormViewModelBase : ObservableViewModelBase
{
    protected readonly ILogger _logger;
    private readonly IDialogService? _dialogService;

    public LoadingOverlayViewModel LoadingOverlay { get; } = new();

    private string _name = string.Empty;
    private DateTime _createdAt = DateTime.Now;
    private string _createdAtText;

    protected FormViewModelBase(ILogger logger, IDialogService? dialogService = null)
    {
        _logger = logger;
        _dialogService = dialogService;
        _createdAtText = _createdAt.ToString(Formatting.DateTimeHuman);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
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

    protected async Task<T?> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string loadingText, string errorMessage)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            var result = await operation();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            if (_dialogService != null)
            {
                await _dialogService.ShowMessageAsync("Ошибка", $"{errorMessage}: {ex.Message}");
            }
            else
            {
                SetError($"{errorMessage}: {ex.Message}");
            }
            return default(T);
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    protected string GetCleanedName()
    {
        return Name.Trim();
    }

    protected DateTime GetCreatedAtUtc()
    {
        return CreatedAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(CreatedAt, DateTimeKind.Local).ToUniversalTime()
            : CreatedAt.ToUniversalTime();
    }

    protected async Task ShowSuccessAsync(string message)
    {
        if (_dialogService != null)
        {
            await _dialogService.ShowMessageAsync("Успех", message);
        }
    }

    protected async Task ShowErrorAsync(string message)
    {
        if (_dialogService != null)
        {
            await _dialogService.ShowMessageAsync("Ошибка", message);
        }
    }

    protected void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }

    public event Action<bool>? WindowClosed;
}

