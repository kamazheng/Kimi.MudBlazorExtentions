// ***********************************************************************
// <copyright file="BaseCrudLayout.razor.cs" company="Molex(Chengdu)">
//     Copyright © Molex(Chengdu) 2025
// </copyright>
// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 03/31/2025
// ***********************************************************************

using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Layouts;

public partial class BaseCrudLayout<T>
{

    [Inject]
    public KimiJsInterop JsInterop { get; set; } = null!;
    [Inject]
    public ISnackbar? _snackbar { get; set; }
    [Parameter]
    public bool AllowDelete { get; set; } = true;

    [Parameter]
    public bool AllowEdit { get; set; } = true;

    [Parameter]
    public bool ShowActionBar { get; set; } = true;

    [Parameter]
    public string EditLable { get; set; } = "Edit";
    [Parameter]
    public string SubmitLable { get; set; } = "Submit";

    [Parameter]
    public String DeleteLable { get; set; } = "Delete";

    [Parameter]
    public bool ShowValidationSummary { get; set; }

    [Parameter, EditorRequired]
    public T Model { get; set; } = default!;

    [Parameter]
    public EventCallback<BaseCrudLayout<T>> OnDelete { get; set; }

    [Parameter]
    public EventCallback<BaseCrudLayout<T>> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment<BaseCrudLayout<T>>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<BaseCrudLayout<T>>? FooterContent { get; set; }

    [Parameter]
    public RenderFragment<BaseCrudLayout<T>>? UserActions { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public bool IsDeleteDisable { get; set; }

    [Parameter]
    [Category(CategoryTypes.FormComponent.Validation)]
    public object? Validation { get; set; }

    /// <summary>
    /// Use the Floating action button instead of normal button.
    /// </summary>
    [Parameter]
    public bool UseFab { get; set; } = false;

    /// <summary>
    /// Fab to be verical with label or not.
    /// </summary>
    [Parameter]
    public bool VirticalFab { get; set; } = true;

    private string baseCrudFormId = TagIdGenerator.Create();
    public MudForm? EditForm { get; set; }

    protected override void OnInitialized()
    {
        Validation ??= FluentValidationService.ValidateValue;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsInterop.setNotScrollMaxHeight(baseCrudFormId, 35);
    }
}
