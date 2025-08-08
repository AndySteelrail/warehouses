using System;
using System.Threading.Tasks;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы с диалогами
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы с диалогами
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Показать модальное окно создания склада
    /// </summary>
    /// <returns>True если склад был создан</returns>
    Task<bool> ShowCreateWarehouseDialogAsync();

    /// <summary>
    /// Показать модальное окно создания площадки
    /// </summary>
    /// <param name="warehouseId">Идентификатор склада</param>
    /// <returns>True если площадка была создана</returns>
    Task<bool> ShowCreatePlatformDialogAsync(int warehouseId);

    /// <summary>
    /// Показать модальное окно создания пикета
    /// </summary>
    /// <param name="warehouseId">Идентификатор склада</param>
    /// <returns>True если пикет был создан</returns>
    Task<bool> ShowCreatePicketDialogAsync(int warehouseId);

    /// <summary>
    /// Показать модальное окно добавления груза
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <returns>True если груз был добавлен</returns>
    Task<bool> ShowAddCargoDialogAsync(int platformId);

    /// <summary>
    /// Показать модальное окно редактирования склада
    /// </summary>
    /// <param name="warehouse">Склад для редактирования</param>
    /// <returns>True если склад был обновлен</returns>
    Task<bool> ShowEditWarehouseDialogAsync(Warehouse warehouse);

    /// <summary>
    /// Показать модальное окно редактирования площадки
    /// </summary>
    /// <param name="platform">Площадка для редактирования</param>
    /// <returns>True если площадка была обновлена</returns>
    Task<bool> ShowEditPlatformDialogAsync(Platform platform);

    /// <summary>
    /// Показать модальное окно редактирования пикета
    /// </summary>
    /// <param name="picket">Пикет для редактирования</param>
    /// <returns>True если пикет был обновлен</returns>
    Task<bool> ShowEditPicketDialogAsync(Picket picket);

    /// <summary>
    /// Показать модальное окно закрытия склада
    /// </summary>
    /// <param name="warehouse">Склад для закрытия</param>
    /// <returns>True если склад был закрыт</returns>
    Task<bool> ShowCloseWarehouseDialogAsync(Warehouse warehouse);

    /// <summary>
    /// Показать модальное окно закрытия пикета
    /// </summary>
    /// <param name="picket">Пикет для закрытия</param>
    /// <returns>True если пикет был закрыт</returns>
    Task<bool> ShowClosePicketDialogAsync(Picket picket);

    /// <summary>
    /// Показать диалог подтверждения удаления
    /// </summary>
    /// <param name="message">Сообщение для подтверждения</param>
    /// <param name="deleteAction">Действие для удаления</param>
    /// <returns>True если пользователь подтвердил удаление</returns>
    Task<bool> ShowDeleteConfirmationAsync(string message, Func<Task> deleteAction);

    /// <summary>
    /// Показать простое сообщение пользователю
    /// </summary>
    /// <param name="title">Заголовок окна</param>
    /// <param name="message">Сообщение</param>
    /// <returns>Task</returns>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Показать сообщение с кнопками
    /// </summary>
    /// <param name="title">Заголовок окна</param>
    /// <param name="message">Сообщение</param>
    /// <param name="buttons">Тип кнопок</param>
    /// <returns>Результат выбора пользователя</returns>
    Task<MessageBoxResult> ShowMessageAsync(string title, string message, MessageBoxButtons buttons);
} 