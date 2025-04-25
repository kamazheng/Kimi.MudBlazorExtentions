using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Buttons;

public class ErrorCatchIconButton : MudIconButton
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    [Parameter]
    public bool Display { get; set; } = true;
    private ProcessingState ProcessingState { get; set; } = new();

    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        await this.ErrorCatchOnClickHandler(_semaphore, Snackbar, DialogService, ProcessingState, () => InvokeAsync(StateHasChanged));
    }
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (Display)
        {
            base.BuildRenderTree(__builder);
        }
    }

}