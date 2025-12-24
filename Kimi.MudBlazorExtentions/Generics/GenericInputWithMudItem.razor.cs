using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class GenericInputWrapMudItem<T> : GenericInput<T>
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<MudItem>(0);
        __builder.AddAttribute(1, "xs", 12);
        __builder.AddAttribute(2, "sm", 6);
        __builder.AddAttribute(3, "md", 4);
        __builder.AddAttribute(4, "lg", 3);
        // Add ChildContent for MudItem
        __builder.AddAttribute(5, "ChildContent", (RenderFragment)(childbuilder =>
        {
            base.BuildRenderTree(childbuilder);
        }));
        __builder.CloseComponent();
    }

}