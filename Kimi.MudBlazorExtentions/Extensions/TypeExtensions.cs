// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

using System.ComponentModel;
using System.Globalization;

namespace Kimi.MudBlazorExtentions.Extensions
{
    /// <summary>
    /// Defines the <see cref="TypeExtensions" />
    /// </summary>
    public static class TypeExtensions
    {
        #region Methods

        internal static T? ChangeType<T>(object? value)
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

            return (T?)Convert.ChangeType(value, t!);
        }
        internal static object? ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value is null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            object result;
            try
            {
                result = Convert.ChangeType(value, t!);
                return result;
            }
            catch (InvalidCastException)
            {
                if (t == typeof(DateTime) && value.ToString()!.IsDouble())
                {
                    return DateTime.FromOADate((double)value);
                }
                return TypeDescriptor.GetConverter(t!).ConvertFromInvariantString(value.ToString()!);
            }
        }
        internal static bool IsDouble(this string theValue)
        {
            double retNum;
            return double.TryParse(theValue, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out retNum);
        }

        public static bool IsTuple(this Type type)
        {
            return type.IsGenericType && type.FullName!.StartsWith("System.ValueTuple");
        }

        #endregion
    }
}
