using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    public partial class SimpleDialog
    {
        [CascadingParameter]
        [NotNull]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public string ContentText { get; set; } = default!;

        [Parameter, EditorRequired]
        public string SubmitButtonText { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public string CancleButtonText { get; set; } = string.Empty;

        [Parameter]
        public Color Color { get; set; }

        private void Submit() => MudDialog.Close(DialogResult.Ok(true));

        private void Cancel() => MudDialog.Cancel();
    }
}