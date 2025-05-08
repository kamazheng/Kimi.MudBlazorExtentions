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

    [Parameter]
    public string? FilterLabel { get; set; } = "Filter";
    [Parameter]
    public string? FieldLabel { get; set; } = "Field";
    [Parameter]
    public string? OperatorLabel { get; set; } = "Operator";
    [Parameter]
    public string? ValueLabel { get; set; } = "Value";

    /// <summary>
    /// Custom display labels for operators. Keys must match the fixed operators: =, &lt;&gt;, &gt;, &lt;, &gt;=, &lt;=, Contains, StartsWith, EndsWith.
    /// Example: new Dictionary&lt;string, string&gt; { { "=", "Equals" }, { "&lt;&gt;", "Not Equals" }, { "Contains", "Includes" } }
    /// If not provided or a key is missing, the default labels are used.
    /// </summary>
    [Parameter]
    public Dictionary<string, string> OperatorLabels { get; set; } = new Dictionary<string, string>
    {
        { "=", "Equals" },
        { "<>", "Not Equals" },
        { ">", "Greater Than" },
        { "<", "Less Than" },
        { ">=", "Greater or Equal" },
        { "<=", "Less or Equal" },
        { "Contains", "Contains" },
        { "StartsWith", "Starts With" },
        { "EndsWith", "Ends With" }
    };

    private PropertyInfo[]? Properties;
    private PropertyInfo SelectedProperty = null!;
    private Type? previousTableClassType;
    private bool _isOpen;

    private Dictionary<string, string> OperatorDisplayList { get; set; } = new();
    private string? SelectedOperator { get; set; }
    private string? InputValue { get; set; }

    private readonly List<string> DefaultStringOperators = new() { "=", "<>", ">", "<", ">=", "<=", "Contains", "StartsWith", "EndsWith" };
    private readonly List<string> DefaultNonStringOperators = new() { "=", "<>", ">", "<", ">=", "<=" };

    protected override async Task OnInitializedAsync()
    {
        Clearable = true;
        if (Adornment == default) Adornment = Adornment.Start;
        if (AdornmentIcon == default) AdornmentIcon = Icons.Material.Filled.Add;
        if (string.IsNullOrEmpty(Label)) Label = "Filter";
        OnAdornmentClick = EventCallback.Factory.Create<MouseEventArgs>(this, TogglePopover);
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
        }
    }

    private async Task ResetData()
    {
        Properties = TableClassType == null ? typeof(T).GetProperties() : TableClassType!.GetProperties();
        SelectedProperty = Properties.Length > 0 ? Properties[0] : throw new InvalidOperationException("Cannot get the table class property!");
        if (SelectedProperty != null)
        {
            FieldSelectedValuesChanged(SelectedProperty);
        }
        if (!string.IsNullOrEmpty(Value))
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

        var operators = typeCode == TypeCode.String ? DefaultStringOperators : DefaultNonStringOperators;
        OperatorDisplayList.Clear();
        foreach (var op in operators)
        {
            OperatorDisplayList[op] = OperatorLabels.GetValueOrDefault(op, op);
        }
    }

    private void FilterAndConfirm(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value))
        {
            Value = GetExpression();
        }
        else
        {
            Value += $" AND {GetExpression()}";
        }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }

    private void FilterOrConfirm(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(Value))
        {
            Value = GetExpression();
        }
        else
        {
            Value += $" OR {GetExpression()}";
        }
        ValueChanged.InvokeAsync(Value);
        _isOpen = false;
    }

    protected virtual string GetExpression()
    {
        if (string.IsNullOrEmpty(SelectedOperator) || SelectedProperty == null)
            return "";

        var name = $"[{SelectedProperty.Name}]";
        var underlyingType = Nullable.GetUnderlyingType(SelectedProperty.PropertyType) ?? SelectedProperty.PropertyType;
        System.TypeCode typeCode = Type.GetTypeCode(underlyingType);

        if (typeCode == TypeCode.String)
        {
            switch (SelectedOperator)
            {
                case "Contains":
                case "StartsWith":
                case "EndsWith":
                    if (string.IsNullOrEmpty(InputValue))
                        throw new InvalidOperationException($"Input value cannot be empty for '{SelectedOperator}' operator.");
                    return BuildLikeClause(name, SelectedOperator, InputValue);
                case "=":
                case "<>":
                    if (string.IsNullOrEmpty(InputValue))
                    {
                        return SelectedOperator == "="
                            ? $"({name} IS NULL OR {name} = N'')"
                            : $"({name} IS NOT NULL AND {name} <> N'')";
                    }
                    else
                    {
                        string formattedValue = FormatFilterValue(InputValue, underlyingType);
                        return $"{name} {SelectedOperator} {formattedValue}";
                    }
                default:
                    throw new InvalidOperationException($"Unsupported operator: {SelectedOperator}");
            }
        }
        else
        {
            if (SelectedOperator == "=" || SelectedOperator == "<>")
            {
                if (string.IsNullOrEmpty(InputValue))
                {
                    return $"{name} IS {(SelectedOperator == "=" ? "" : "NOT ")}NULL";
                }
                else
                {
                    string formattedValue = FormatFilterValue(InputValue, underlyingType);
                    return $"{name} {SelectedOperator} {formattedValue}";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(InputValue))
                    throw new InvalidOperationException($"Input value cannot be empty for '{SelectedOperator}' operator.");
                string formattedValue = FormatFilterValue(InputValue, underlyingType);
                return $"{name} {SelectedOperator} {formattedValue}";
            }
        }
    }

    private static string FormatFilterValue(string? inputValue, Type type)
    {
        if (string.IsNullOrEmpty(inputValue))
            return "NULL";

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return $"N'{inputValue.Replace("'", "''")}'";
        else if (underlyingType == typeof(bool))
        {
            if (inputValue.ToLower() == "true")
                return "1";
            else if (inputValue.ToLower() == "false")
                return "0";
            else
                throw new InvalidOperationException("Invalid boolean value.");
        }
        else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateOnly) || underlyingType == typeof(TimeOnly))
            return $"'{inputValue}'";
        else
            return inputValue;
    }

    private static string BuildLikeClause(string name, string operatorType, string value)
    {
        string pattern = operatorType switch
        {
            "Contains" => $"%{EscapeLikeValue(value)}%",
            "StartsWith" => $"{EscapeLikeValue(value)}%",
            "EndsWith" => $"%{EscapeLikeValue(value)}",
            _ => throw new ArgumentException("Invalid operator type for LIKE clause.")
        };

        return $"{name} LIKE N'{pattern}' ESCAPE '\\'";
    }

    private static string EscapeLikeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_")
            .Replace("[", "\\[")
            .Replace("]", "\\]");
    }

    private bool RequiresInputValue()
    {
        return SelectedOperator != "=" && SelectedOperator != "<>";
    }
}