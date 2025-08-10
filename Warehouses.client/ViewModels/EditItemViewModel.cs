using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels
{
    public abstract class EditItemViewModel : FormViewModelBase
    {
        private string _itemName = string.Empty;
        private int _itemId;

        protected EditItemViewModel(IDialogService dialogService, ILogger logger) : base(logger, dialogService)
        {
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
        }

        private async Task SaveAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var success = await SaveItemAsync();
                if (!success)
                {
                    throw new Exception(ErrorMessageText);
                }
                await ShowSuccessAsync(SuccessMessage);
                CloseWindow(true);
                return true;
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


    }
}
