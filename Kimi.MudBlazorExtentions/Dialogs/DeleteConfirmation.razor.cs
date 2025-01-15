// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Kimi.MudBlazorExtentions.Dialogs
{
    /// <summary>
    /// Defines the <see cref="DeleteConfirmation" />
    /// </summary>
    public partial class DeleteConfirmation
    {
        #region Properties

        /// <summary>
        /// Gets or sets the CancelButtonText
        /// </summary>
        [Parameter, EditorRequired]
        public string? CancelButtonText { get; set; }

        /// <summary>
        /// Gets or sets the ConfirmButtonText
        /// </summary>
        [Parameter, EditorRequired]
        public string? ConfirmButtonText { get; set; }

        /// <summary>
        /// Gets or sets the ContentText
        /// </summary>
        [Parameter]
        public string? ContentText { get; set; }

        /// <summary>
        /// Gets or sets the TitleText
        /// </summary>
        [Parameter, EditorRequired]
        public string? TitleText { get; set; }

        /// <summary>
        /// Gets or sets the MudDialog
        /// </summary>
        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = default!;

        #endregion

        #region Methods

        /// <summary>
        /// The Cancel
        /// </summary>
        private void Cancel() => MudDialog.Cancel();

        /// <summary>
        /// The Submit
        /// </summary>
        private void Submit()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

        #endregion
    }
}
