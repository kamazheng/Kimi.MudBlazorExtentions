
// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/15/2025
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Globalization;

namespace Kimi.MudBlazorExtentions.Extensions;

/// <summary>
/// Defines the <see cref="TypeExtensions"/>
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// 非泛型通用类型转换，支持 Nullable&lt;T&gt;，并增强 DateOnly/TimeOnly/Enum/Guid/Uri/OADate。
    /// </summary>
    public static object? ChangeType(this object? value, Type conversion, IFormatProvider? provider = null)
    {
        if (conversion == null) throw new ArgumentNullException(nameof(conversion));

        // Nullable<T> 处理
        var t = conversion;
        var underlying = Nullable.GetUnderlyingType(t);
        var targetType = underlying ?? t;

        // null 输入：对可空返回 null；对不可空抛异常更清晰
        if (value is null)
        {
            if (underlying != null) return null;
            throw new InvalidCastException($"无法将 null 转为非可空类型 {targetType}。");
        }

        // 已经是目标类型或其派生
        if (targetType.IsInstanceOfType(value))
            return value;

        provider ??= CultureInfo.CurrentCulture;
        var culture = provider as CultureInfo ?? CultureInfo.CurrentCulture;

        // --- 专门支持 DateOnly ---
        if (targetType == typeof(DateOnly))
            return ConvertToDateOnly(value, culture);

        // --- 专门支持 TimeOnly ---
        if (targetType == typeof(TimeOnly))
            return ConvertToTimeOnly(value, culture);

        // --- 枚举 ---
        if (targetType.IsEnum)
            return ConvertToEnum(value, targetType, culture);

        // --- Guid ---
        if (targetType == typeof(Guid))
            return ConvertToGuid(value);

        // --- Uri ---
        if (targetType == typeof(Uri))
            return ConvertToUri(value);

        // --- DateTime OADate（允许数字 / 字符串数字）---
        if (targetType == typeof(DateTime) && IsNumericOrNumericString(value))
        {
            var d = ToDouble(value, culture);
            return DateTime.FromOADate(d);
        }

        // 首选：Convert.ChangeType
        try
        {
            return System.Convert.ChangeType(value, targetType, provider);
        }
        catch (InvalidCastException)
        {
            // 兜底：TypeDescriptor（更广泛的类型转换器）
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null)
            {
                // 优先使用类型直接转换
                if (converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(null, culture, value);
                }

                // 再尝试字符串（Invariant）
                var s = value.ToString();
                if (s != null)
                {
                    try
                    {
                        return converter.ConvertFromInvariantString(s);
                    }
                    catch
                    {
                        // ignore, fall through
                    }
                }
            }

            throw; // 保留原异常语义
        }
    }

    /// <summary>
    /// 泛型版本，内部复用非泛型实现。
    /// </summary>
    public static T? ChangeType<T>(this object? value, IFormatProvider? provider = null)
    {
        var obj = ChangeType(value, typeof(T), provider);
        return (T?)obj;
    }

    // ----------------- 现有扩展保留 -----------------

    public static bool IsDouble(this string theValue)
    {
        double retNum;
        return double.TryParse(theValue, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out retNum);
    }

    public static bool IsTuple(this Type type)
    {
        return type.IsGenericType && type.FullName!.StartsWith("System.ValueTuple");
    }

    // ----------------- 格式定义（可根据业务调整顺序与内容） -----------------

    private static readonly string[] DateOnlyFormats =
    {
        "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
        "MM/dd/yyyy", "dd/MM/yyyy",
        "yyyyMMdd"   // 紧凑格式
    };

    private static readonly string[] TimeOnlyFormats =
    {
        "HH:mm:ss", "HH:mm", "HHmmss",
        "h\\:mm\\:ss", "h\\:mm",     // TimeSpan 风格
        "hh:mm tt", "hh:mm:ss tt"    // 12 小时制
    };

    // ----------------- 专用转换实现 -----------------

    private static DateOnly ConvertToDateOnly(object value, IFormatProvider provider)
    {
        switch (value)
        {
            case DateOnly d:
                return d;

            case DateTime dt:
                return DateOnly.FromDateTime(dt);

            case DateTimeOffset dto:
                // 用本地时间或按需要改为 dto.UtcDateTime
                return DateOnly.FromDateTime(dto.LocalDateTime);

            case string s:
                // 先 TryParse（支持多语言/本地化）
                if (DateOnly.TryParse(s, provider, DateTimeStyles.None, out var d1))
                    return d1;

                // 再尝试常用格式（ParseExact）
                foreach (var fmt in DateOnlyFormats)
                {
                    if (DateOnly.TryParseExact(s, fmt, provider, DateTimeStyles.None, out var d2))
                        return d2;
                }

                throw new FormatException($"无法解析为 DateOnly：\"{s}\"。");

            default:
                throw new InvalidCastException($"不支持将类型 {value.GetType()} 转为 DateOnly。");
        }
    }

    private static TimeOnly ConvertToTimeOnly(object value, IFormatProvider provider)
    {
        switch (value)
        {
            case TimeOnly t:
                return t;

            case TimeSpan ts:
                return TimeOnly.FromTimeSpan(ts);

            case DateTime dt:
                return TimeOnly.FromDateTime(dt);

            case DateTimeOffset dto:
                return TimeOnly.FromDateTime(dto.LocalDateTime);

            case string s:
                // 先 TryParse
                if (TimeOnly.TryParse(s, provider, DateTimeStyles.None, out var t1))
                    return t1;

                // 再尝试常用格式（ParseExact）
                foreach (var fmt in TimeOnlyFormats)
                {
                    if (TimeOnly.TryParseExact(s, fmt, provider, DateTimeStyles.None, out var t2))
                        return t2;
                }

                // 再尝试把字符串当 TimeSpan（例如 "1:02:03"）
                if (TimeSpan.TryParse(s, provider, out var ts2))
                    return TimeOnly.FromTimeSpan(ts2);

                throw new FormatException($"无法解析为 TimeOnly：\"{s}\"。");

            default:
                throw new InvalidCastException($"不支持将类型 {value.GetType()} 转为 TimeOnly。");
        }
    }

    // ----------------- 数值/OA Date 辅助 -----------------

    private static bool IsNumericOrNumericString(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong
               or float or double or decimal
               || (value is string s && double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _));
    }

    private static double ToDouble(object value, IFormatProvider provider)
    {
        var culture = provider as CultureInfo ?? CultureInfo.CurrentCulture;

        return value switch
        {
            double d => d,
            float f => f,
            decimal m => (double)m,
            string s when double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dv) => dv,
            _ => System.Convert.ToDouble(value, culture)
        };
    }

    // ----------------- 其它类型专用转换 -----------------

    private static object ConvertToEnum(object value, Type enumType, IFormatProvider provider)
    {
        if (value is string s)
        {
            // 允许名称、逗号分隔的 Flags 名称，忽略大小写
            return Enum.Parse(enumType, s, ignoreCase: true);
        }

        // 数值 -> 枚举
        var numeric = System.Convert.ChangeType(value, Enum.GetUnderlyingType(enumType), provider);
        return Enum.ToObject(enumType, numeric!);
    }

    private static object ConvertToGuid(object value)
    {
        if (value is Guid g) return g;
        if (value is string gs) return Guid.Parse(gs);
        throw new InvalidCastException($"无法将类型 {value.GetType()} 转为 Guid。");
    }

    private static object ConvertToUri(object value)
    {
        if (value is Uri u) return u;
        if (value is string us) return new Uri(us, UriKind.RelativeOrAbsolute);
        throw new InvalidCastException($"无法将类型 {value.GetType()} 转为 Uri。");
    }
}