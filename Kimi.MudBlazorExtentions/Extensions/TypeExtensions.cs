// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

namespace Kimi.MudBlazorExtentions.Extensions
{
    /// <summary>
    /// Defines the <see cref="TypeExtensions" />
    /// </summary>
    public static class TypeExtensions
    {
        #region Methods

        /// <summary>
        /// The ChangeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value<see cref="object"/></param>
        /// <returns>The <see cref="T?"/></returns>
        internal static T? ChangeType<T>(object value)
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t!);
        }

        #endregion
    }
}
