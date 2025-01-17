using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class FieldInfoInput
{
    [Parameter]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter, EditorRequired]
    public object TupleInstance { get; set; } = null!;
    [Parameter]
    public EventCallback<object> TupleInstanceChanged { get; set; }

    [Parameter, EditorRequired]
    public FieldInfo FieldInfo { get; set; } = null!;


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
}