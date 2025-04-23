using Kimi.MudBlazorExtentions.Dialogs;
using Kimi.MudBlazorExtentions.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Layouts;

public class ErrorCatchButton : MudButton
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    [Parameter]
    public bool Display { get; set; } = true;

    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        // Attempt to acquire the semaphore without blocking
        if (!await _semaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            // Call the base button click handler
            await base.OnClickHandler(ev);
        }
        catch (ReturnException returnEx) when (Snackbar is not null)
        {
            // Handle ReturnException by showing a snackbar message
            Snackbar.Info(returnEx.Message);
        }
        catch (Exception ex) when (DialogService is not null)
        {
            // Show an error dialog for other exceptions
            var parameters = new DialogParameters<MyErrorContent>
            {
                { x => x.CurrentException, ex },
                { x => x.IfDialog, true }
            };

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true
            };

            var dialog = await DialogService.ShowAsync<MyErrorContent>("", parameters, options);
            await dialog.Result;
        }
        finally
        {
            // Ensure the semaphore is always released
            _semaphore.Release();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (Display)
        {
            base.BuildRenderTree(__builder);
        }
    }

}