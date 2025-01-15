using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    /// <summary>
    /// A dialog component for selecting multiple items.
    /// </summary>
    public partial class MultiSelectDialog<T>
    {
        [CascadingParameter]
        [NotNull]
        private MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter, EditorRequired]
        public string CancelButtonText { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public string SubmitButtonText { get; set; } = string.Empty;

        [Parameter]
        public Color Color { get; set; }

        [Parameter]
        public List<SelectDialogItem<T>> SelectionItems { get; set; } = new List<SelectDialogItem<T>>();

        public List<T> SelectedValues { get; set; } = new List<T>();

        private string searchText = string.Empty;
        private List<SelectDialogItem<T>> FilteredItems => string.IsNullOrWhiteSpace(searchText)
            ? SelectionItems
            : SelectionItems.Where(item => item.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

        private void Submit()
        {
            SelectedValues = SelectionItems.Where(item => item.IsSelected).Select(item => item.Value).ToList();
            MudDialog.Close(DialogResult.Ok(SelectedValues));
        }

        private void Cancel() => MudDialog.Cancel();

        private void SelectAll()
        {
            foreach (var item in FilteredItems)
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        private void FilterItems()
        {
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}