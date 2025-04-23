using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class DynamicOrderBy<T> : MudTextField<string>
{
    [Parameter]
    public Type? TableClassType { get; set; }

    [Parameter]
    public Origin AnchorOrigin { get; set; }

    [Parameter]
    public Origin TransformOrigin { get; set; }

    [Parameter]
    public int? ZIndex { get; set; }

    [Parameter]
    public string? AscendLabel { get; set; }

    [Parameter]
    public string? DescendLabel { get; set; }

    private PropertyInfo[]? Properties;
    private PropertyInfo SelectedProperty = null!;
    private Type? previousTableClassType;


    private bool _isOpen;


    protected override async Task OnInitializedAsync()
    {
        this.Clearable = true;
        if (Adornment == default) this.Adornment = Adornment.Start;
        if (AdornmentIcon == default) this.AdornmentIcon = Icons.Material.Filled.Add;
        if (string.IsNullOrEmpty(Label)) this.Label = "Order By";
        this.OnAdornmentClick = EventCallback.Factory.Create<MouseEventArgs>(this, TogglePopover);
        ZIndex ??= 1000;
        AscendLabel ??= "Ascend";
        DescendLabel ??= "Descend";
        await Task.CompletedTask;
    }
    protected override async Task OnParametersSetAsync()
    {
        if (previousTableClassType != TableClassType)
        {
            previousTableClassType = TableClassType;
            await ResetData();
            await Task.CompletedTask;
        }
    }

    private async Task ResetData()
    {
        Properties = TableClassType == null ? typeof(T).GetProperties() : TableClassType!.GetProperties();
        SelectedProperty = Properties[0];
        if (Value != "")
        {
            Value = "";
            await ValueChanged.InvokeAsync(Value);
        }
    }

    public void TogglePopover(MouseEventArgs eventArgs)
    {
        _isOpen = !_isOpen;
    }

    private void OnAscend(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value)) { Value = $"[{SelectedProperty.Name}]"; } else { Value += $",[{SelectedProperty.Name}]"; }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }

    private void OnDescend(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value)) { Value = $"[{SelectedProperty.Name}] DESC"; } else { Value += $",[{SelectedProperty.Name}] DESC"; }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }
}

