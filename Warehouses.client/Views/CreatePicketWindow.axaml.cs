using Avalonia.Controls;
using System.Threading.Tasks;

namespace Warehouses.client.Views;

/// <summary>
/// Окно создания пикета
/// </summary>
public partial class CreatePicketWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();
    
    public CreatePicketWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
    }

    public void SetResult(bool result)
    {
        _resultCompletionSource.TrySetResult(result);
        Close();
    }

    public async Task<bool> GetResultAsync()
    {
        return await _resultCompletionSource.Task;
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_resultCompletionSource.Task.IsCompleted)
        {
            _resultCompletionSource.TrySetResult(false);
        }
    }
} 