using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Warehouses.client.Views
{
    public partial class EditItemWindow : Window
    {
        private TaskCompletionSource<bool> _resultCompletionSource = new();

        public EditItemWindow()
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
}
