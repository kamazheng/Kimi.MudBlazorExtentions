using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    public partial class SelectDialog<T>
    {
        [CascadingParameter]
        [NotNull]
        private MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter, EditorRequired]
        public string CancelButtonText { get; set; } = string.Empty;

        [Parameter]
        public Color Color { get; set; }

        [Parameter]
        public List<SelectDialogItem<T>> SelectionItems { get; set; } = new List<SelectDialogItem<T>>();

        public T SelectedValue { get; set; } = default!;

        private void Submit() => MudDialog.Close(DialogResult.Ok(SelectedValue));

        private void Cancel() => MudDialog.Cancel();

        protected override void OnInitialized()
        {
            SelectedValue = SelectionItems!.FirstOrDefault()!.Value;
            base.OnInitialized();
        }
    }
}