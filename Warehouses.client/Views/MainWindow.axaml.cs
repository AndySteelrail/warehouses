using Avalonia.Controls;
using Avalonia.Input;
using Warehouses.client.Models;
using Warehouses.client.ViewModels;

namespace Warehouses.client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnNodeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is TreeNode node)
        {
            if (node.NodeType == TreeNodeType.CreateWarehouse)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.EditCommand.Execute(node);
                }
            }
        }
    }
}