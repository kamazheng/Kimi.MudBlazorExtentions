// ***********************************************************************
// <copyright file="DynamicFilter.razor.cs" company="Molex(Chengdu)">
//     Copyright © Molex(Chengdu) 2025
// </copyright>
// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 03/19/2025
// ***********************************************************************

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class DynamicFilter<T>
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
    public string? AndLabel { get; set; }

    [Parameter]
    public string? OrLabel { get; set; }

    private PropertyInfo[]? Properties;
    private PropertyInfo SelectedProperty = null!;
    private Type? previousTableClassType;

    private bool _isOpen;

    //filter generator
    private List<string> Operators { get; set; } = new List<string>() { "==", "!=", ">", "<", ">=", "<=", "Contains", "StartsWith", "EndsWith" };

    private string? SelectedOperator { get; set; }
    private string? InputValue { get; set; }


    protected override async Task OnInitializedAsync()
    {
        this.Clearable = true;
        if (Adornment == default) this.Adornment = Adornment.Start;
        if (AdornmentIcon == default) this.AdornmentIcon = Icons.Material.Filled.Add;
        if (string.IsNullOrEmpty(Label)) this.Label = "Filter";
        this.OnAdornmentClick = EventCallback.Factory.Create<MouseEventArgs>(this, TogglePopover);
        ZIndex ??= 1000;
        AndLabel ??= "And";
        OrLabel ??= "Or";
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
        FieldSelectedValuesChanged(SelectedProperty);
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

    private void FieldSelectedValuesChanged(PropertyInfo renderField)
    {
        SelectedProperty = renderField;
        InputValue = null;
        var underlyingType = Nullable.GetUnderlyingType(SelectedProperty.PropertyType) ?? SelectedProperty.PropertyType;
        System.TypeCode typeCode = Type.GetTypeCode(underlyingType);
        switch (typeCode)
        {
            case TypeCode.String:
                Operators = new List<string>() { "==", "!=", ">", "<", ">=", "<=", "Contains", "StartsWith", "EndsWith" };
                break;
            default:
                Operators = new List<string>() { "==", "!=", ">", "<", ">=", "<=" };
                break;
        }
    }

    private void FilterAndConfirm(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value)) { Value = getExp(); } else { Value += $" and {getExp()}"; }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }
    private void FilterOrConfirm(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value)) { Value = getExp(); } else { Value += $" or {getExp()}"; }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }
    private string getExp()
    {
        var name = $"[{SelectedProperty!.Name}]";
        var underlyingType = Nullable.GetUnderlyingType(SelectedProperty.PropertyType) ?? SelectedProperty.PropertyType;
        System.TypeCode typeCode = Type.GetTypeCode(underlyingType);
        if (string.IsNullOrEmpty(SelectedOperator))
        {
            return "";
        }
        switch (SelectedOperator)
        {
            case "Contains":
                if (typeCode == TypeCode.String)
                {
                    return $"{name} LIKE '%{InputValue?.ToString()}%'";
                }
                throw new InvalidOperationException("Contains operator is only valid for string types.");

            case "StartsWith":
                if (typeCode == TypeCode.String)
                {
                    return $"{name} LIKE '{InputValue?.ToString()}%'";
                }
                throw new InvalidOperationException("StartsWith operator is only valid for string types.");

            case "EndsWith":
                if (typeCode == TypeCode.String)
                {
                    return $"{name} LIKE '%{InputValue?.ToString()}'";
                }
                throw new InvalidOperationException("EndsWith operator is only valid for string types.");

            default:
                if (typeCode == TypeCode.Single ||
                    typeCode == TypeCode.Double ||
                    typeCode == TypeCode.Decimal ||
                    typeCode == TypeCode.Int16 ||
                    typeCode == TypeCode.Int32 ||
                    typeCode == TypeCode.Int64)
                {
                    // Handle numeric types
                    if (string.IsNullOrEmpty(InputValue) && (SelectedOperator == "!=" || SelectedOperator == "=="))
                    {
                        return $"({name} IS {SelectedOperator.Replace("==", "=")} NULL)";
                    }
                    else
                    {
                        return $"{name} {SelectedOperator.Replace("==", "=")} {InputValue?.ToString()}";
                    }
                }
                else
                {
                    // Handle string or other types
                    if (string.IsNullOrEmpty(InputValue) && SelectedOperator == "!=")
                    {
                        return $"(({name} {SelectedOperator.Replace("==", "=")} '') OR ({name} IS NOT NULL))";
                    }
                    else if (string.IsNullOrEmpty(InputValue) && SelectedOperator == "==")
                    {
                        return $"(({name} {SelectedOperator.Replace("==", "=")} '') OR ({name} IS NULL))";
                    }
                    else
                    {
                        return $"{name} {SelectedOperator.Replace("==", "=")} '{InputValue?.ToString()}'";
                    }
                }
        }
    }
}

