using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Buttons;

public class ErrorCatchButton : MudButton
{
    // 实例级：仅防同一按钮重复点击。⚠️ 禁止改回 static——全局共享会导致"打开对话框的按钮 await 期间，
    // 静态锁被占满，对话框内的 ErrorCatchButton 点击 WaitAsync(0) 失败而静默无反应"。
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Parameter]
    public bool Display { get; set; } = true;

    private ProcessingState ProcessingState { get; set; } = new();

    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        await this.ErrorCatchOnClickHandler(_semaphore, Snackbar, DialogService, ProcessingState, () => InvokeAsync(StateHasChanged), Navigation);
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (Display)
        {
            base.BuildRenderTree(__builder);
        }
    }

}