using Microsoft.JSInterop;

namespace Kimi.MudBlazorExtentions;

// This class provides an example of how JavaScript functionality can be wrapped in a .NET class
// for easy consumption. The associated JavaScript module is loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor components
// for use.

public class KimiJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> kimiTask;
    private readonly IJSRuntime jsRuntime;

    public KimiJsInterop(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
        kimiTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Kimi.MudBlazorExtentions/kimi.js").AsTask());
    }

    public async ValueTask SetPageTitle(string title)
    {
        var module = await kimiTask.Value;
        await module.InvokeVoidAsync("setTitle", new object[] { title });
    }

    public async ValueTask setCkEditorHtml(string html)
    {
        await jsRuntime.InvokeVoidAsync("editor.setData", new object[] { html });
    }

    public async ValueTask SaveAsFile(string fileName, byte[] fileByte)
    {
        var module = await kimiTask.Value;
        var basw64str = Convert.ToBase64String(fileByte);
        await module.InvokeAsync<string>("saveAsFile", new object[] { fileName, basw64str });
    }

    public async ValueTask SetNotScrollMaxHeight(string id, int desiredMargin, string minHeight = "600px")
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setNotScrollMaxHeight", new object[] { id, desiredMargin, minHeight });
    }

    public async ValueTask SetMinHeightToMaxWindowsHeight(string id, int desiredMargin)
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setMinHeightToMaxWindowsHeight", new object[] { id, desiredMargin });
    }

    public async ValueTask SetNotScrollMaxHeightByClass(string className, int desiredMargin, string minHeight = "600px")
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setNotScrollMaxHeightByClass", new object[] { className, desiredMargin, minHeight });
    }

    public async ValueTask SetDivReadonlyByDivId(string id, bool isReadOnly)
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setDivReadOnlyByDivId", new object[] { id, isReadOnly });
    }

    public async ValueTask SetDivReadonlyByDivClassName(string divClassName, bool isReadOnly)
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setDivReadOnlyByDivClassName", new object[] { divClassName, isReadOnly });
    }

    public async ValueTask removeDiv(string divId)
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("removeDiv", new object[] { divId });
    }

    public async ValueTask SetAllSubElementsWithSameSelector(string divId)
    {
        var module = await kimiTask.Value;
        await module.InvokeAsync<string>("setAllSubElementsWithSameSelector", new object[] { divId });
    }

    public async ValueTask DisposeAsync()
    {
        if (kimiTask.IsValueCreated)
        {
            var module = await kimiTask.Value;
            await module.DisposeAsync();
        }
    }
}