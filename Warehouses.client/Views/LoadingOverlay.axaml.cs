using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Warehouses.client.Views
{
    public partial class LoadingOverlay : UserControl
    {
        public LoadingOverlay()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
