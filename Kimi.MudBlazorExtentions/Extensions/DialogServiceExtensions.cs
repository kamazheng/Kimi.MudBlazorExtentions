// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

using Kimi.MudBlazorExtentions.Dialogs;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Extensions
{
    /// <summary>
    /// Defines the <see cref="DialogServiceExtensions" />
    /// </summary>
    public static class DialogServiceExtensions
    {
        #region Methods

        /// <summary>
        /// Displays a confirmation dialog with the specified content text
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="contentText">The content text</param>
        /// <returns>The dialog result</returns>
        public static async Task<bool> ConfirmV2Async(this IDialogService dialogService, string contentText)
        {
            var cf = await dialogService.ConfirmAsync("Confirm", contentText, Color.Info, "Confirm", "Cancel");
            return !cf.Canceled;
        }
        /// <summary>
        /// Displays a confirmation dialog with the specified title, content text, confirm button text, cancel button text, and color
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="title">The title</param>
        /// <param name="contentText">The content text</param>
        /// <param name="color">The color</param>
        /// <param name="confirmBtnText">The confirm button text</param>
        /// <param name="cancelBtnText">The cancel button text</param>
        /// <param name="maxwidth"></param>
        /// <param name="fullWidth"></param>
        /// <returns>The dialog result</returns>
        public static async Task<bool> ConfirmV2Async(this IDialogService dialogService, string title, string contentText, Color color = Color.Info, string confirmBtnText = "Confirm", string cancelBtnText = "Cancel", MaxWidth maxwidth = MaxWidth.Small, bool fullWidth = true)
        {
            var cf = await dialogService.ConfirmAsync(title, contentText, color, confirmBtnText, cancelBtnText, maxwidth, fullWidth);
            return !cf.Canceled;
        }

        public static async Task ConfirmReturnExceptionAsync(this IDialogService dialogService, string title, string contentText, Color color = Color.Info, string confirmBtnText = "Confirm", string cancelBtnText = "Cancel", MaxWidth maxwidth = MaxWidth.Small, bool fullWidth = true)
        {
            var cf = await dialogService.ConfirmAsync(title, contentText, color, confirmBtnText, cancelBtnText, maxwidth, fullWidth);
            if (cf.Canceled) throw new ReturnException("User canceled the operation");
        }

        /// <summary>
        /// Displays a confirmation dialog with the specified content text
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="contentText">The content text</param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> ConfirmAsync(this IDialogService dialogService, string contentText)
        {
            return await dialogService.ConfirmAsync("Confirm", contentText, Color.Info, "Confirm", "Cancel");
        }

        /// <summary>
        /// Displays a confirmation dialog with the specified title, content text, confirm button text, cancel button text, and color
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="title">The title</param>
        /// <param name="contentText">The content text</param>
        /// <param name="color">The color</param>
        /// <param name="confirmBtnText">The confirm button text</param>
        /// <param name="cancelBtnText">The cancel button text</param>
        /// <param name="maxwidth"></param>
        /// <param name="fullWidth"></param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> ConfirmAsync(this IDialogService dialogService, string title, string contentText, Color color = Color.Info, string confirmBtnText = "Confirm", string cancelBtnText = "Cancel", MaxWidth maxwidth = MaxWidth.Small, bool fullWidth = true)
        {
            var parameters = new DialogParameters
                    {
                        {nameof(SimpleDialog.ContentText), contentText },
                        {nameof(SimpleDialog.SubmitButtonText), confirmBtnText },
                        {nameof(SimpleDialog.CancleButtonText), cancelBtnText },
                        {nameof(SimpleDialog.Color), color }
                    };
            var options = new DialogOptions { CloseButton = true, MaxWidth = maxwidth, FullWidth = fullWidth };
            var dialog = await dialogService.ShowAsync<SimpleDialog>(title, parameters, options);
            var result = await dialog.Result;
            return result ?? DialogResult.Cancel();
        }

        /// <summary>
        /// Displays an error dialog with the specified content text
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="contentText">The content text</param>
        /// <param name="title"></param>
        /// <param name="confirmBtnText"></param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> ErrorAsync(this IDialogService dialogService, string contentText, string title = "ERROR", string confirmBtnText = "CONFIRM")
        {
            return await dialogService.ConfirmAsync(title, contentText, Color.Error, confirmBtnText, cancelBtnText: "");
        }

        /// <summary>
        /// Displays an info dialog with the specified content text
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="contentText">The content text</param>
        /// <param name="title"></param>
        /// <param name="confirmBtnText"></param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> InfoAsync(this IDialogService dialogService, string contentText, string title = "INFO", string confirmBtnText = "CONFIRM")
        {
            return await dialogService.ConfirmAsync(title, contentText, Color.Info, confirmBtnText, cancelBtnText: "");
        }

        /// <summary>
        /// Displays an input box dialog with the specified title, content text, color, and width
        /// </summary>
        /// <typeparam name="T">The type of the input value</typeparam>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="title">The title</param>
        /// <param name="contentText">The content text</param>
        /// <param name="inputContent"></param>
        /// <param name="color">The color</param>
        /// <param name="width">The width</param>
        /// <param name="submitBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <param name="labels">if T is tuple, this is for the tuple field label</param>
        /// <returns>The dialog result</returns>
        public static async Task<T?> InputBoxAsync<T>(this IDialogService dialogService, string title, string contentText, T? inputContent = default, Color color = Color.Info, MaxWidth width = MaxWidth.Small, string submitBtnText = "Submit", string cancelBtnText = "Cancel", string[]? labels = default)
        {
            if (typeof(T) == typeof(DateTime) && (inputContent is null || (DateTime)(object)inputContent == DateTime.MinValue))
            {
                inputContent = (T)(object)DateTime.Now;
            }
            var parameters = new DialogParameters
                    {
                        {nameof(InputDialog<int>.ContentText), contentText },
                        {nameof(InputDialog<int>.InputContent), inputContent },
                        {nameof(InputDialog<int>.SubmitButtonText), submitBtnText },
                        {nameof(InputDialog<int>.CancelButtonText), cancelBtnText },
                        {nameof(InputDialog<int>.TupleLabels), labels },
                        {nameof(InputDialog<int>.Color), color },
                    };
            var options = new DialogOptions { CloseButton = true, MaxWidth = width, FullWidth = true };
            var dialog = await dialogService.ShowAsync<InputDialog<T>>(title, parameters, options);
            var result = await dialog.Result;
            return result?.Data == null ? default : (T?)result?.Data;
        }

        /// <summary>
        /// Displays a multi-select box dialog with the specified title, selection items, color, and width
        /// </summary>
        /// <typeparam name="T">The type of the selection items</typeparam>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="title">The title</param>
        /// <param name="selectionItems">The selection items</param>
        /// <param name="color">The color</param>
        /// <param name="width">The width</param>
        /// <param name="confirmBtnText">The confirm button text</param>
        /// <param name="cancelBtnText">The cancel button text</param>
        /// <returns>The dialog result</returns>
        public static async Task<List<T>?> MultiSelectBoxAsync<T>(this IDialogService dialogService, string title, List<SelectDialogItem<T>> selectionItems, Color color = Color.Default, MaxWidth width = MaxWidth.Small
            , string confirmBtnText = "Confirm", string cancelBtnText = "Cancel")
        {
            var parameters = new DialogParameters
                    {
                        { nameof(MultiSelectDialog<int>.CancelButtonText), cancelBtnText },
                        { nameof(MultiSelectDialog<int>.SubmitButtonText), confirmBtnText },
                        { nameof(MultiSelectDialog<int>.SelectionItems), selectionItems },
                        { nameof(MultiSelectDialog<int>.Color), color }
                    };
            var options = new DialogOptions { CloseButton = true, MaxWidth = width, FullWidth = true };
            var dialog = await dialogService.ShowAsync<MultiSelectDialog<T>>(title, parameters, options);
            var result = await dialog.Result;
            return result?.Data == null ? default : (List<T>?)result?.Data;
        }

        /// <summary>
        /// The RequiredInputBoxAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dialogService">The dialogService<see cref="IDialogService"/></param>
        /// <param name="title">The title<see cref="string"/></param>
        /// <param name="contentText">The contentText<see cref="string"/></param>
        /// <param name="inputContent"></param>
        /// <param name="color">The color<see cref="Color"/></param>
        /// <param name="width">The width<see cref="MaxWidth"/></param>
        /// <param name="submitBtnText">The submitBtnText<see cref="string"/></param>
        /// <param name="cancelBtnText">The cancelBtnText<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> RequiredInputBoxAsync<T>(this IDialogService dialogService, string title, string contentText, T? inputContent = default, Color color = Color.Info, MaxWidth width = MaxWidth.Small
             , string submitBtnText = "Submit", string cancelBtnText = "Cancel")
        {
            var tResult = await dialogService.InputBoxAsync(title, contentText, inputContent, color, width, submitBtnText, cancelBtnText);
            if (tResult is null)
            {
                throw new InvalidDataException("No input has been provided");
            }
            return tResult;
        }


        public static async Task<List<T>> RequiredMultiSelectBoxAsync<T>(this IDialogService dialogService, string title, List<SelectDialogItem<T>> selectionItems, Color color, MaxWidth width = MaxWidth.Small
            , string confirmBtnText = "Confirm")
        {
            var tResult = await dialogService.MultiSelectBoxAsync(title, selectionItems, color, width, confirmBtnText, "");
            if (tResult == null)
            {
                throw new InvalidDataException("No items have been selected");
            }
            return tResult;
        }


        public static async Task<T> RequiredSelectBoxAsync<T>(this IDialogService dialogService, string title, List<SelectDialogItem<T>> selectionItems, Color color = Color.Default, MaxWidth width = MaxWidth.Small
            , string cancelBtnText = "Cancel")
        {
            var result = await dialogService.SelectBoxAsync(title, selectionItems, color, width, cancelBtnText);
            var tResult = (T?)result?.Data;
            if (tResult is null)
            {
                throw new InvalidDataException("No item has been selected");
            }
            return tResult;
        }

        /// <summary>
        /// Displays a select box dialog with the specified title, selection items, color, and width
        /// </summary>
        /// <typeparam name="T">The type of the selection items</typeparam>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="title">The title</param>
        /// <param name="selectionItems">The selection items</param>
        /// <param name="color">The color</param>
        /// <param name="width">The width</param>
        /// <param name="cancelBtnText"></param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> SelectBoxAsync<T>(this IDialogService dialogService, string title, List<SelectDialogItem<T>> selectionItems, Color color, MaxWidth width = MaxWidth.Small
            , string cancelBtnText = "Cancel")
        {
            var parameters = new DialogParameters
                    {
                        { nameof(SelectDialog<int>.CancelButtonText), cancelBtnText },
                        { nameof(SelectDialog<int>.SelectionItems), selectionItems },
                        { nameof(SelectDialog<int>.Color), color }
                    };
            var options = new DialogOptions { CloseButton = true, MaxWidth = width, FullWidth = true };
            var dialog = await dialogService.ShowAsync<SelectDialog<T>>(title, parameters, options);
            var result = await dialog.Result;
            return result ?? DialogResult.Cancel();
        }

        /// <summary>
        /// Displays an alert dialog with the specified title, content text, and color
        /// </summary>
        /// <param name="dialogService">The dialog service</param>
        /// <param name="contentText">The content text</param>
        /// <param name="title">The title</param>
        /// <param name="confirmBtnText"></param>
        /// <returns>The dialog result</returns>
        public static async Task<DialogResult> WarningAsync(this IDialogService dialogService, string contentText, string title = "WARNING", string confirmBtnText = "CONFIRM")
        {
            return await dialogService.ConfirmAsync(title, contentText, Color.Warning, confirmBtnText, cancelBtnText: "");
        }

        #endregion
    }
}
