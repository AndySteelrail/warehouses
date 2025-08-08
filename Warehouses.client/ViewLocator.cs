using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using Warehouses.client.ViewModels;

namespace Warehouses.client;

/// <summary>
/// Локатор для связывания ViewModels с Views
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Создать View для ViewModel
    /// </summary>
    /// <param name="data">ViewModel</param>
    /// <returns>View</returns>
    public Control Build(object? data)
    {
        if (data == null)
            return new TextBlock { Text = "No data" };

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Проверить, может ли шаблон обработать данные
    /// </summary>
    /// <param name="data">Данные</param>
    /// <returns>True если может</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}