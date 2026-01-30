using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class GenericInputWrapMudItem<T> : GenericInput<T>
{
    [Parameter]
    public bool IsItemDense { get; set; } = false;
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<KimiMudItem>(0);
        __builder.AddComponentParameter(1, nameof(KimiMudItem.IsDense), IsItemDense);
        __builder.AddAttribute(2, "ChildContent", (RenderFragment)(childbuilder =>
        {
            base.BuildRenderTree(childbuilder);
        }));
        __builder.CloseComponent();
    }

}

public class KimiMudItem : MudItem
{
    [Parameter]
    public bool IsDense { get; set; } = false;

    protected override void OnInitialized()
    {
        if (IsDense)
        {
            // 紧凑模式：适合图标、小控件、列表项等
            xs = 12;
            sm = 6;
            md = 4;
            lg = 2;
            xl = 2;
            xxl = 1;
        }
        else
        {
            xs = 12;
            sm = 6;
            md = 6;
            lg = 3;
            xl = 3;
            xxl = 2;
        }

        base.OnInitialized();
    }
}