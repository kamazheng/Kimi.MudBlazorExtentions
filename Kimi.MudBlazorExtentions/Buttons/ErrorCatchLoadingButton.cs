﻿using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Buttons;

public class ErrorCatchLoadingButton : MudButton
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar? Snackbar { get; set; }

    [Parameter]
    public bool Display { get; set; } = true;

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.Check; // Default icon

    [Parameter, EditorRequired]
    public string Label { get; set; } = "Submit"; // Button label
    private ProcessingState ProcessingState { get; set; } = new();

    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        await this.ErrorCatchOnClickHandler(_semaphore, Snackbar, DialogService, ProcessingState, () => InvokeAsync(StateHasChanged));
    }
    protected override async Task OnInitializedAsync()
    {
        StartIcon = null;
        EndIcon = null;
        await Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (Display)
        {
            // Set the ChildContent for the base button
            ChildContent = builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "d-flex align-center");

                if (ProcessingState.IsProcessing)
                {
                    // Render loading animation and text
                    builder.OpenComponent<MudProgressCircular>(2);
                    builder.AddAttribute(3, "Class", "ms-n1");
                    builder.AddAttribute(4, "Size", Size.Small);
                    builder.AddAttribute(5, "Indeterminate", true);
                    builder.CloseComponent();

                    builder.OpenComponent<MudText>(6);
                    builder.AddAttribute(7, "Class", "ms-2");
                    builder.AddAttribute(8, "ChildContent", (RenderFragment)((textBuilder) => textBuilder.AddContent(9, $"{Label}...")));
                    builder.CloseComponent();
                }
                else
                {
                    // Render icon and text
                    builder.OpenComponent<MudIcon>(10);
                    builder.AddAttribute(11, "Icon", Icon); // Use the Icon parameter
                    builder.CloseComponent();

                    builder.OpenComponent<MudText>(12);
                    builder.AddAttribute(13, "ChildContent", (RenderFragment)((textBuilder) => textBuilder.AddContent(14, $"{Label}")));
                    builder.CloseComponent();
                }

                builder.CloseElement(); // Close the <span> element
            };

            // Call the base BuildRenderTree to render the button with the updated ChildContent
            base.BuildRenderTree(__builder);
        }
    }
}