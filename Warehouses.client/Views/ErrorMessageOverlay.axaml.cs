using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Warehouses.client.Views
{
    public partial class ErrorMessageOverlay : UserControl
    {
        public ErrorMessageOverlay()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

