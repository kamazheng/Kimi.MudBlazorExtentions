// ***********************************************************************
// ***********************************************************************
// Author           : kama zheng
// Created          : 04/24/2025
// ***********************************************************************

using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Buttons;

public class ErrorCatchLoadingFab : MudFab
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    [Parameter]
    public bool Display { get; set; } = true;

    /// <summary>
    /// If not vertical using the native MudFab, otherwise, using the MudStack verical layout.
    /// </summary>
    [Parameter]
    public bool Vertical { get; set; } = true;

    private string? StackClass { get; set; }
    private string? TextLable { get; set; }
    private ProcessingState ProcessingState { get; set; } = new();


    protected override void OnParametersSet()
    {
        if (Vertical)
        {
            StackClass = Class;
            Class = string.Empty;
            TextLable = Label;
            Label = string.Empty;
        }
        base.OnParametersSet();
    }
    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        await this.ErrorCatchOnClickHandler(_semaphore, Snackbar, DialogService, ProcessingState, () => InvokeAsync(StateHasChanged));
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (!Display)
        {
            return; // Skip rendering if Display is false
        }

        if (!Vertical)
        {
            if (ProcessingState.IsProcessing)
            {
                renderProcessing(__builder);
            }
            else
            {
                base.BuildRenderTree(__builder);
            }
            return;
        }
        // Render the MudStack layout
        __builder.OpenComponent<MudStack>(0);
        __builder.AddAttribute(1, "AlignItems", AlignItems.Center);
        __builder.AddAttribute(2, "Justify", Justify.Center);
        __builder.AddAttribute(3, "Spacing", 1);
        __builder.AddAttribute(4, "Class", StackClass);

        // Add ChildContent for MudStack
        __builder.AddAttribute(5, "ChildContent", (RenderFragment)(builder =>
        {
            if (ProcessingState.IsProcessing)
            {
                renderProcessing(builder);
            }
            else
            {
                base.BuildRenderTree(builder);
            }

            // Render the label below the MudFab
            builder.OpenComponent<MudText>(6);
            builder.AddAttribute(70, "Align", Align.Center);
            builder.AddAttribute(80, "Typo", Typo.caption);
            builder.AddAttribute(90, "ChildContent", (RenderFragment)(labelBuilder =>
            {
                labelBuilder.AddContent(100, TextLable);
            }));
            builder.CloseComponent();
        }));

        // Close the MudStack
        __builder.CloseComponent();
    }

    private void renderProcessing(RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<MudProgressCircular>(11);
        __builder.AddAttribute(12, "Class", "ms-n1");
        __builder.AddAttribute(13, "Size", Size);
        __builder.AddAttribute(14, "Indeterminate", true);
        __builder.CloseComponent();
    }
}
