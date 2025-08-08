using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Warehouses.client.Services;
using Warehouses.client.ViewModels;
using Warehouses.client.Views;

namespace Warehouses.client;

/// <summary>
/// Главный класс приложения
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Инициализация приложения
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    /// <summary>
    /// Настройка жизненного цикла приложения
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var mainViewModel = _serviceProvider!.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Настройка сервисов и DI контейнера
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Настройка логирования
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // HTTP клиент и API сервис
        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001/");
        });

        // Остальные сервисы
        services.AddSingleton<IWarehouseService, WarehouseService>();
        services.AddSingleton<IPlatformService, PlatformService>();
        services.AddSingleton<IPicketService, PicketService>();
        services.AddSingleton<IReferenceService, CargoTypesService>();
        
        services.AddSingleton<ICargoService, CargoService>();
        services.AddSingleton<IDialogService, DialogService>();
        
        // Новые сервисы для декомпозиции
        services.AddSingleton<TreeDataService>();
        services.AddSingleton<MainWindowStateManager>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<WarehousesViewModel>();
        services.AddTransient<CreateWarehouseViewModel>();
        services.AddTransient<CreatePlatformViewModel>();
        services.AddTransient<CreatePicketViewModel>();
        services.AddTransient<AddCargoViewModel>();
        services.AddTransient<EditWarehouseViewModel>();
        services.AddTransient<EditPlatformViewModel>();
        services.AddTransient<EditPicketViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}