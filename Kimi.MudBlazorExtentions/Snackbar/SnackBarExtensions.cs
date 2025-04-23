// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/16/2025
// ***********************************************************************

using MudBlazor;

namespace Kimi.MudBlazorExtentions.Snackbar
{
    /// <summary>
    /// Defines the <see cref="SnackbarExtensions" />
    /// </summary>
    public static class SnackbarExtensions
    {
        #region Methods

        /// <summary>
        /// The Error
        /// </summary>
        /// <param name="snackBar">The snackBar<see cref="ISnackbar"/></param>
        /// <param name="msg">The msg<see cref="string"/></param>
        public static void Error(this ISnackbar snackBar, string msg)
        {
            snackBar.Add(msg, Severity.Error);
        }

        /// <summary>
        /// The Info
        /// </summary>
        /// <param name="snackBar">The snackBar<see cref="ISnackbar"/></param>
        /// <param name="msg">The msg<see cref="string"/></param>
        public static void Info(this ISnackbar snackBar, string msg)
        {
            snackBar.Add(msg, Severity.Info);
        }

        /// <summary>
        /// The ShowException
        /// </summary>
        /// <param name="snackBar">The snackBar<see cref="ISnackbar"/></param>
        /// <param name="ex">The ex<see cref="Exception"/></param>
        public static void Exception(this ISnackbar snackBar, Exception ex)
        {
            snackBar.Add($"{ex.Message}\n{ex.InnerException?.Message}", Severity.Error);
        }

        /// <summary>
        /// The Warning
        /// </summary>
        /// <param name="snackBar">The snackBar<see cref="ISnackbar"/></param>
        /// <param name="msg">The msg<see cref="string"/></param>
        public static void Warning(this ISnackbar snackBar, string msg)
        {
            snackBar.Add(msg, Severity.Warning);
        }

        /// <summary>
        /// The Warning
        /// </summary>
        /// <param name="snackBar">The snackBar<see cref="ISnackbar"/></param>
        /// <param name="msg">The msg<see cref="string"/></param>
        public static void Success(this ISnackbar snackBar, string msg)
        {
            snackBar.Add(msg, Severity.Success);
        }

        #endregion
    }
}
