using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class FieldInfoInput
{
    [Parameter, EditorRequired]
    public object TupleInstance { get; set; } = null!;
    [Parameter]
    public EventCallback<object> TupleInstanceChanged { get; set; }

    [Parameter, EditorRequired]
    public FieldInfo FieldInfo { get; set; } = null!;

    private bool Clearable { get; set; }
    private bool isNullable => Nullable.GetUnderlyingType(FieldInfo.FieldType) != null;


    private System.TypeCode TypeCode => Type.GetTypeCode(FieldInfo.FieldType);
    private Type? underlyingType => Nullable.GetUnderlyingType(FieldInfo.FieldType) ?? FieldInfo.FieldType;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TupleInstance == null)
        {
            var hostType = FieldInfo.DeclaringType;
            if (hostType != null)
            {
                TupleInstance = Activator.CreateInstance(hostType)!;
                TupleInstanceChanged.InvokeAsync(TupleInstance);
            }
        }
    }


    private double? ToDoubleOrDefault(object? val)
    {
        if (val is null && isNullable == true) return null;
        if (val is null && isNullable == false) return 0;
        var s = val?.ToString();
        return Convert.ToDouble(s, CultureInfo.InvariantCulture);
    }

}