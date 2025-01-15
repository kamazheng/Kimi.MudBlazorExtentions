using MudBlazor;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    public static class SnackBarExtensions
    {
        public static void ShowException(this ISnackbar snackBar, Exception? ex)
        {
            snackBar.Add($"{ex.Message}\n{ex.InnerException?.Message}", Severity.Error);
        }
    }
}