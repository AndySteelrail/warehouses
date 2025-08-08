using Avalonia.Controls;
using System.Threading.Tasks;

namespace Warehouses.client.Views;

/// <summary>
/// Окно добавления груза
/// </summary>
public partial class AddCargoWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();
    
    public AddCargoWindow()
    {
        InitializeComponent();
        
        this.Closed += (sender, e) =>
        {
            if (!_resultCompletionSource.Task.IsCompleted)
            {
                _resultCompletionSource.SetResult(false);
            }
        };
    }
    
    public Task<bool> GetResultAsync()
    {
        return _resultCompletionSource.Task;
    }
    
    public void SetResult(bool result)
    {
        _resultCompletionSource.SetResult(result);
        Close();
    }
} 