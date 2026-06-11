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
                // 先复位 loading 状态，再等用户关闭对话框；
                // 否则对话框打开期间 IsProcessing 仍为 true，loading 遮罩会盖住错误对话框。
                processingState.IsProcessing = false;
                await stateHasChanged.Invoke();
                await ApiErrorPresenter.PresentAsync(ex, Snackbar, DialogService, Navigation);
            }
            finally
            {
                processingState.IsProcessing = false; // 幂等，catch 已重置；此处保底
                await stateHasChanged.Invoke();
                _semaphore.Release();
            }
        }
    }
}