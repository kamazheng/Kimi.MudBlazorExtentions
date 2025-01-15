using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    public partial class InputDialog<T>
    {
        [CascadingParameter]
        [NotNull]
        private MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public string ContentText { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public string SubmitButtonText { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public string CancelButtonText { get; set; } = string.Empty;

        [Parameter]
        public Color Color { get; set; }

        [Parameter]
        public T? InputContent { get; set; }

        private MudTextField<T>? textInput;
        private MudNumericField<T>? numberInput;

        private void Submit() => MudDialog.Close(DialogResult.Ok(InputContent));

        private void Cancel() => MudDialog.Cancel();
    }
}