using Avalonia.Controls;
using Avalonia.Interactivity;
using Warehouses.client.ViewModels;
using System.Threading.Tasks;

namespace Warehouses.client.Views;

/// <summary>
/// Окно создания площадки
/// </summary>
public partial class CreatePlatformWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();
    
    public CreatePlatformWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        if (DataContext is CreatePlatformViewModel viewModel)
        {
            // Загружаем пикеты при открытии окна
            _ = viewModel.LoadPicketsAsync();
        }
    }

    public void SetResult(bool result)
    {
        _resultCompletionSource.TrySetResult(result);
        Close();
    }

    public Task<bool> GetResultAsync()
    {
        return _resultCompletionSource.Task;
    }
} 