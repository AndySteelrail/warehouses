using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Warehouses.client.Models;
using Warehouses.client.ViewModels;
using Warehouses.client.Views;
using Microsoft.Extensions.Logging;

namespace Warehouses.client.Services;

/// <summary>
/// Реализация сервиса для работы с диалогами
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    
    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<bool> ShowCreateWarehouseDialogAsync()
    {
        var viewModel = _serviceProvider.GetRequiredService<CreateWarehouseViewModel>();
        var window = new CreateWarehouseWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowCreatePlatformDialogAsync(int warehouseId)
    {
        var platformService = _serviceProvider.GetRequiredService<IPlatformService>();
        var picketService = _serviceProvider.GetRequiredService<IPicketService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<CreatePlatformViewModel>>();
        var viewModel = new CreatePlatformViewModel(platformService, picketService, this, logger)
        {
            WarehouseId = warehouseId
        };
        var window = new CreatePlatformWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowCreatePicketDialogAsync(int warehouseId)
    {
        var picketService = _serviceProvider.GetRequiredService<IPicketService>();
        var platformService = _serviceProvider.GetRequiredService<IPlatformService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<CreatePicketViewModel>>();
        var viewModel = new CreatePicketViewModel(picketService, platformService, this, warehouseId, logger);
        var window = new CreatePicketWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowAddCargoDialogAsync(int platformId)
    {
        var referenceService = _serviceProvider.GetRequiredService<IReferenceService>();
        var cargoService = _serviceProvider.GetRequiredService<ICargoService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<AddCargoViewModel>>();
        var viewModel = new AddCargoViewModel(referenceService, cargoService, this, logger, platformId);
        var window = new AddCargoWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowEditWarehouseDialogAsync(Warehouse warehouse)
    {
        var warehouseService = _serviceProvider.GetRequiredService<IWarehouseService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<EditWarehouseViewModel>>();
        var viewModel = new EditWarehouseViewModel(warehouseService, this, logger);
        viewModel.Initialize(warehouse);
        
        var window = new EditItemWindow();
        window.DataContext = viewModel;
        
        viewModel.WindowClosed += (result) => window.SetResult(result);
        
        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowEditPlatformDialogAsync(Platform platform)
    {
        var platformService = _serviceProvider.GetRequiredService<IPlatformService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<EditPlatformViewModel>>();
        var viewModel = new EditPlatformViewModel(platformService, this, logger);
        viewModel.Initialize(platform);
        
        var window = new EditItemWindow();
        window.DataContext = viewModel;
        
        viewModel.WindowClosed += (result) => window.SetResult(result);
        
        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowEditPicketDialogAsync(Picket picket)
    {
        var picketService = _serviceProvider.GetRequiredService<IPicketService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<EditPicketViewModel>>();
        var viewModel = new EditPicketViewModel(picketService, this, logger);
        viewModel.Initialize(picket);
        
        var window = new EditItemWindow();
        window.DataContext = viewModel;
        
        viewModel.WindowClosed += (result) => window.SetResult(result);
        
        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowCloseWarehouseDialogAsync(Warehouse warehouse)
    {
        var warehouseService = _serviceProvider.GetRequiredService<IWarehouseService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<CloseWarehouseViewModel>>();
        var viewModel = new CloseWarehouseViewModel(warehouseService, this, logger, warehouse);
        
        var window = new CloseWarehouseWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowClosePicketDialogAsync(Picket picket)
    {
        var picketService = _serviceProvider.GetRequiredService<IPicketService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<ClosePicketViewModel>>();
        var viewModel = new ClosePicketViewModel(picketService, this, logger, picket);
        
        var window = new ClosePicketWindow
        {
            DataContext = viewModel
        };
        
        viewModel.WindowClosed += (result) => window.SetResult(result);

        window.Show();
        return await window.GetResultAsync();
    }
    
    public async Task<bool> ShowDeleteConfirmationAsync(string message, Func<Task> deleteAction)
    {
        var result = await ShowMessageBoxAsync("Подтверждение удаления", message, MessageBoxButtons.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            await deleteAction();
            return true;
        }
        return false;
    }
    
    public async Task ShowMessageAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message, MessageBoxButtons.Ok);
    }
    
    public async Task<MessageBoxResult> ShowMessageAsync(string title, string message, MessageBoxButtons buttons)
    {
        return await ShowMessageBoxAsync(title, message, buttons);
    }
    
    private async Task<MessageBoxResult> ShowMessageBoxAsync(string title, string message, MessageBoxButtons buttons)
    {
        var tcs = new TaskCompletionSource<MessageBoxResult>();
        
        var window = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var textBlock = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(20),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 10, 0, 20),
            Spacing = 10
        };

        // Создаем кнопки в зависимости от типа
        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Height = 30
                };
                okButton.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.Yes);
                    window.Close();
                };
                buttonPanel.Children.Add(okButton);
                break;

            case MessageBoxButtons.YesNo:
                var yesButton = new Button
                {
                    Content = "Да",
                    Width = 80,
                    Height = 30
                };
                yesButton.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.Yes);
                    window.Close();
                };

                var noButton = new Button
                {
                    Content = "Нет",
                    Width = 80,
                    Height = 30
                };
                noButton.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.No);
                    window.Close();
                };

                buttonPanel.Children.Add(yesButton);
                buttonPanel.Children.Add(noButton);
                break;

            case MessageBoxButtons.YesNoCancel:
                var yesButton2 = new Button
                {
                    Content = "Да",
                    Width = 80,
                    Height = 30
                };
                yesButton2.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.Yes);
                    window.Close();
                };

                var noButton2 = new Button
                {
                    Content = "Нет",
                    Width = 80,
                    Height = 30
                };
                noButton2.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.No);
                    window.Close();
                };

                var cancelButton = new Button
                {
                    Content = "Отмена",
                    Width = 80,
                    Height = 30
                };
                cancelButton.Click += (sender, e) =>
                {
                    tcs.SetResult(MessageBoxResult.Cancel);
                    window.Close();
                };

                buttonPanel.Children.Add(yesButton2);
                buttonPanel.Children.Add(noButton2);
                buttonPanel.Children.Add(cancelButton);
                break;
        }

        var stackPanel = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        stackPanel.Children.Add(textBlock);
        stackPanel.Children.Add(buttonPanel);

        window.Content = stackPanel;
        window.Show();

        return await tcs.Task;
    }
}

public enum MessageBoxResult
{
    Yes,
    No,
    Cancel
}

public enum MessageBoxButtons
{
    YesNo,
    YesNoCancel,
    Ok
} 