using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Extensions
{
    // Define a simple wrapper for IsProcessing
    public class ProcessingState
    {
        public bool IsProcessing { get; set; }
    }

    public static class MudButtonExtensions
    {
        public static async Task ErrorCatchOnClickHandler(
            this MudBaseButton button,
            SemaphoreSlim _semaphore,
            ISnackbar? Snackbar,
            IDialogService? DialogService,
            ProcessingState processingState,
            Func<Task> stateHasChanged,
            NavigationManager? Navigation = null
            )
        {
            if (!await _semaphore.WaitAsync(0)) return;

            try
            {
                // Check and update IsProcessing via the wrapper
                if (processingState.IsProcessing) return;
                processingState.IsProcessing = true;
                await stateHasChanged.Invoke();

                // Invoke the button's OnClick handler
                await button.OnClick.InvokeAsync();
            }
            catch (Exception ex)
            {
                // 统一分流：ReturnException/403/401/网络/5xx/真 bug 都走集中 presenter
                await ApiErrorPresenter.PresentAsync(ex, Snackbar, DialogService, Navigation);
            }
            finally
            {
                // Reset IsProcessing in the wrapper
                processingState.IsProcessing = false;
                await stateHasChanged.Invoke();
                // Release the semaphore
                _semaphore.Release();
            }
        }
    }
}