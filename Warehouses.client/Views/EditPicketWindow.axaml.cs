using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Warehouses.client.ViewModels;

namespace Warehouses.client.Views;

/// <summary>
/// Окно редактирования пикета
/// </summary>
public partial class EditPicketWindow : Window
{
    private TaskCompletionSource<bool> _resultCompletionSource = new();
    
    public EditPicketWindow()
    {
        InitializeComponent();
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
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        SetResult(false);
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditPicketViewModel viewModel)
        {
            viewModel.SaveCommand.Execute(null);
        }
    }
} 