using Microsoft.AspNetCore.Components;

namespace Kimi.MudBlazorExtentions.Layouts;

public interface ITabHomePage
{
    /// <summary>
    /// Pass the parameter and type to the parent component to add a new tab. Must add [Parameter] attribute for implementation.
    /// Typical usage:
    /// ComponentType => the blazor page type of the component to be added, 
    /// Parameters => the parameters to be passed to the component,
    /// Title => the new tab title,
    /// OnClose => the function to be called when the tab before closing,
    /// var parameters = new Dictionary&lt;string, object&gt; {{ 'Parameter1', 'NewValue1' },{ 'Parameter2', 456 }};
    /// await AddNewTabCallback.InvokeAsync((parameters, typeof(TabContent)));
    /// </summary>
    EventCallback<(Type ComponentType, Dictionary<string, object> Parameters, string Title, Func<string, bool, Task<bool>> OnClose)> AddNewTabCallback { get; set; }
}