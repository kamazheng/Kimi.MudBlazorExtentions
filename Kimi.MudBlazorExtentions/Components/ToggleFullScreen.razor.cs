namespace Kimi.MudBlazorExtentions.Components;

using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class ToggleFullScreen
{

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; }


    private string divId = TagIdGenerator.Create();

    private bool isFullScreen = false;

    private async Task toggleFullScreenAsync(MouseEventArgs e)
    {
        // var js = @"
        //     (async function() {
        //         const element = document.getElementById('contentBlockId');
        //         if (!document.fullscreenElement) {
        //             await element.requestFullscreen().catch(err => console.error('Fullscreen request failed:', err));
        //         } else {
        //             await document.exitFullscreen().catch(err => console.error('Exit fullscreen failed:', err));
        //         }
        //         element.classList.toggle('fullscreen');
        //     })();
        //     ";

        var js = @"
            (async function() {
                const element = document.getElementById('contentBlockId');
                element.classList.toggle('fullscreen');
            })();
            ";
        js = js.Replace("contentBlockId", divId);
        await _jsRuntime.InvokeVoidAsync("eval", js);
        isFullScreen = !isFullScreen;
    }
}