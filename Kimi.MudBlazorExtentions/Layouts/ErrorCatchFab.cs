// ***********************************************************************
// <copyright file="ErrorCatchFab.cs" company="Molex(Chengdu)">
//     Copyright © Molex(Chengdu) 2025
// </copyright>
// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 04/24/2025
// ***********************************************************************

using Kimi.MudBlazorExtentions.Dialogs;
using Kimi.MudBlazorExtentions.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Layouts;

public class ErrorCatchFab : MudFab
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private bool _isProcessing;

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

    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                InvokeAsync(StateHasChanged);
            }
        }
    }

    protected override void OnParametersSet()
    {
        if (Vertical)
        {
            this.StackClass = this.Class;
            this.Class = string.Empty;
            this.TextLable = this.Label;
            this.Label = string.Empty;
        }
        base.OnParametersSet();
    }
    protected override async Task OnClickHandler(MouseEventArgs ev)
    {
        if (!await _semaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            if (IsProcessing)
            {
                return;
            }

            IsProcessing = true;
            await base.OnClickHandler(ev);
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
            IsProcessing = false;
            _semaphore.Release();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        if (!Display)
        {
            return; // Skip rendering if Display is false
        }
        if (!Vertical)
        {
            base.BuildRenderTree(__builder);
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
            // Render the MudFab itself
            base.BuildRenderTree(builder); // Delegate rendering of MudFab to the base class

            // Render the label below the MudFab
            builder.OpenComponent<MudText>(6);
            builder.AddAttribute(7, "Align", Align.Center);
            builder.AddAttribute(8, "Typo", Typo.caption);
            builder.AddAttribute(9, "ChildContent", (RenderFragment)(labelBuilder =>
            {
                labelBuilder.AddContent(10, TextLable);
            }));
            builder.CloseComponent();
        }));

        // Close the MudStack
        __builder.CloseComponent();
    }
}
