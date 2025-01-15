namespace Kimi.MudBlazorExtentions.Dialogs
{
    public class SelectDialogItem<T>
    {
        public T Value { get; set; } = default!;
        public string Text { get; set; } = default!;
        public bool IsSelected { get; internal set; }
    }
}