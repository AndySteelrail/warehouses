using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels
{
    public abstract class EditItemViewModel : ModalViewModelBase
    {
        private readonly ILogger _logger;
        private string _itemName = string.Empty;
        private int _itemId;
        
        public LoadingOverlayViewModel LoadingOverlay { get; } = new();

        protected EditItemViewModel(IDialogService dialogService, ILogger logger) : base(dialogService)
        {
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
            await ExecuteWithOverlayAsync(async () =>
            {
                var success = await SaveItemAsync();
                if (!success)
                {
                    throw new Exception(ErrorMessageText);
                }
                _logger.LogInformation("Элемент успешно обновлен: Id={Id}, NewName={Name}", ItemId, ItemName);
                await ShowSuccessAsync(SuccessMessage);
                CloseWindow(true);
            }, LoadingText, ErrorMessageText);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ItemName) && !LoadingOverlay.IsVisible;
        }

        private void Cancel()
        {
            CloseWindow(false);
        }
        
        protected abstract Task<bool> SaveItemAsync();

        private async Task<bool> ExecuteWithOverlayAsync(Func<Task> action, string loadingText, string errorPrefix)
        {
            try
            {
                LoadingOverlay.LoadingText = loadingText;
                LoadingOverlay.IsVisible = true;
                ClearError();
                await action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorPrefix);
                await ShowErrorAsync($"{errorPrefix}: {ex.Message}");
                return false;
            }
            finally
            {
                LoadingOverlay.IsVisible = false;
            }
        }
    }
}
