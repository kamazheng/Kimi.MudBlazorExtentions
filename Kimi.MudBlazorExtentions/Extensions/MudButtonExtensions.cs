using Kimi.MudBlazorExtentions.Layouts;
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
            Func<Task> stateHasChanged
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
            catch (ReturnException ex) when (Snackbar != null)
            {
                Snackbar.Info(ex.Message);
            }
            catch (Exception value) when (DialogService != null)
            {
                var dialogParameters = new DialogParameters<MyErrorContent>
                {
                    { x => x.CurrentException, value },
                    { x => x.IfDialog, true }
                };

                var options = new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true
                };

                await (await DialogService.ShowAsync<MyErrorContent>("", dialogParameters, options)).Result;
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