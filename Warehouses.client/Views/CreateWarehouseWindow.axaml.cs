using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Warehouses.client.ViewModels;

namespace Warehouses.client.Views;

/// <summary>
/// Окно создания склада
/// </summary>
public partial class CreateWarehouseWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();
    
    public CreateWarehouseWindow()
    {
        InitializeComponent();
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
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        SetResult(false);
    }
    
    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CreateWarehouseViewModel viewModel)
        {
            viewModel.CreateCommand.Execute(null);
        }
    }
} 