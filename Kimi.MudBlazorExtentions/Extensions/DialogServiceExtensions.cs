// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

using Kimi.MudBlazorExtentions.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Extensions
{
    /// <summary>
    /// Defines the <see cref="DialogServiceExtensions" />
    /// </summary>
    public static class DialogServiceExtensions
    {

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
        /// <param name="labels">if T is tuple, this is for the tuple field label. Otherwise, only show the perperty which's name in labels</param>
        /// <returns>The dialog result</returns>
        public static async Task<T?> InputBoxAsync<T>(this IDialogService dialogService, string title, string contentText,
                T? inputContent = default, Color color = Color.Info, MaxWidth width = MaxWidth.Small,
                string submitBtnText = "Submit", string cancelBtnText = "Cancel", string[]? labels = default)
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

        /// <summary>
        /// 返回载荷的版本：用对话框包裹页面，返回 TResult。
        /// - 若页面接收共享模型（如参数名为“Model”），外层提交时返回该模型；
        /// - 若页面自行在内部调用 Close(Ok(payload))，则此方法返回 payload。
        /// </summary>
        public static async Task<TResult?> ShowPageAsync<TResult, TPage>(
            this IDialogService dialogService,
            string title,
            string? contentText = null,
            TResult? sharedModel = default,                // 可选：外部共享模型
            string modelParamName = "",               // 页面参数名（如已有 Model）
            Dictionary<string, object>? pageParameters = null,
            Color color = Color.Info,
            MaxWidth width = MaxWidth.Large,
            string submitBtnText = "Submit",
            string cancelBtnText = "Cancel",
            bool showActions = true                        // 若页面自行关闭，设为 false
        )
            where TPage : ComponentBase
        {
            var (dialog, result) = await ShowPageCoreAsync<TResult, TPage>(
                dialogService, title, contentText, sharedModel, modelParamName,
                pageParameters, color, width, submitBtnText, cancelBtnText, showActions);

            // 取消则返回 default；否则尝试拿到 TResult 载荷
            if (result is null || result.Canceled)
                return default;

            return result.Data is TResult typed ? typed : default;
        }

        /// <summary>
        /// 不返回值的版本：仅显示页面，不关心结果。
        /// 内部复用返回值版本的实现（以 object 作为载荷类型）。
        /// </summary>
        public static async Task ShowPageAsync<TPage>(
            this IDialogService dialogService,
            string title,
            string? contentText = null,
            Dictionary<string, object>? pageParameters = null,
            Color color = Color.Info,
            MaxWidth width = MaxWidth.Large,
            string submitBtnText = "Submit",
            string cancelBtnText = "Cancel",
            bool showActions = true
        )
            where TPage : ComponentBase
        {
            // 直接调用返回值版本，载荷类型用 object；调用方忽略结果即可
            _ = await ShowPageAsync<object, TPage>(
                dialogService,
                title,
                contentText,
                sharedModel: null,          // 不传共享模型
                modelParamName: "",         // 不注入模型参数
                pageParameters,
                color,
                width,
                submitBtnText,
                cancelBtnText,
                showActions
            );
        }

        /// <summary>
        /// 共享的核心实现：构造参数并打开对话框，返回 (dialogHandle, dialogResult)。
        /// 供两个公开方法复用。
        /// </summary>
        private static async Task<(IDialogReference dialog, DialogResult? result)> ShowPageCoreAsync<TResult, TPage>(
            IDialogService dialogService,
            string title,
            string? contentText,
            TResult? sharedModel,
            string modelParamName,
            Dictionary<string, object>? pageParameters,
            Color color,
            MaxWidth width,
            string submitBtnText,
            string cancelBtnText,
            bool showActions
        )
            where TPage : ComponentBase
        {
            pageParameters ??= new();

            // 若提供了共享模型，按页面参数名注入（页面需定义相同参数名）
            if (sharedModel is not null && !string.IsNullOrWhiteSpace(modelParamName))
                pageParameters[modelParamName] = sharedModel;

            var dialogParameters = new DialogParameters
            {
                { nameof(PageDialog<TPage, TResult>.ContentText), contentText ?? string.Empty },
                { nameof(PageDialog<TPage, TResult>.SubmitButtonText), submitBtnText },
                { nameof(PageDialog<TPage, TResult>.CancelButtonText), cancelBtnText },
                { nameof(PageDialog<TPage, TResult>.Color), color },
                { nameof(PageDialog<TPage, TResult>.ChildParameters), pageParameters },
                { nameof(PageDialog<TPage, TResult>.ShowActions), showActions },
                { nameof(PageDialog<TPage, TResult>.SharedModel), sharedModel },
            };

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = width,
                FullWidth = true
            };

            var dialog = await dialogService.ShowAsync<PageDialog<TPage, TResult>>(title, dialogParameters, options);
            var result = await dialog.Result;
            return (dialog, result);
        }
    }
}