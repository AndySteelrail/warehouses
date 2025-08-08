using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Warehouses.client.ViewModels;
using System.Threading.Tasks;

namespace Warehouses.client.Views;

public partial class CloseWarehouseWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();

    public CloseWarehouseWindow()
    {
        AvaloniaXamlLoader.Load(this);
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
