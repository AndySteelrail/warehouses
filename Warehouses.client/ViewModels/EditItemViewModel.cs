using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels
{
    public abstract class EditItemViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private string _itemName = string.Empty;
        private int _itemId;
        
        public LoadingOverlayViewModel LoadingOverlay { get; } = new();

        protected EditItemViewModel(IDialogService dialogService, ILogger logger)
        {
            _dialogService = dialogService;
            _logger = logger;
            
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string ItemName
        {
            get => _itemName;
            set
            {
                SetProperty(ref _itemName, value);
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        public int ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public event Action<bool>? WindowClosed;
        
        public abstract string WindowTitle { get; }
        public abstract string LabelText { get; }
        public abstract string WatermarkText { get; }
        public abstract string LoadingText { get; }
        public abstract string SuccessMessage { get; }
        public abstract string ErrorMessageText { get; }

        public void Initialize(int id, string name)
        {
            ItemId = id;
            ItemName = name;
            _logger.LogInformation("Инициализация редактирования: Id={Id}, Name={Name}", id, name);
        }

        private async Task SaveAsync()
        {
            try
            {
                LoadingOverlay.LoadingText = LoadingText;
                LoadingOverlay.IsVisible = true;

                var success = await SaveItemAsync();

                if (success)
                {
                    _logger.LogInformation("Элемент успешно обновлен: Id={Id}, NewName={Name}", ItemId, ItemName);
                    await _dialogService.ShowMessageAsync("Успех", SuccessMessage);
                    WindowClosed?.Invoke(true);
                }
                else
                {
                    _logger.LogError("Не удалось обновить элемент: Id={Id}, NewName={Name}", ItemId, ItemName);
                    await _dialogService.ShowMessageAsync("Ошибка", ErrorMessageText);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении элемента: Id={Id}, NewName={Name}", ItemId, ItemName);
                await _dialogService.ShowMessageAsync("Ошибка", ex.Message);
            }
            finally
            {
                LoadingOverlay.IsVisible = false;
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ItemName) && !LoadingOverlay.IsVisible;
        }

        private void Cancel()
        {
            WindowClosed?.Invoke(false);
        }

        // Абстрактный метод для сохранения
        protected abstract Task<bool> SaveItemAsync();
    }
}
