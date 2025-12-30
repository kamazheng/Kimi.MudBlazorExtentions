
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
    public static object? ChangeType(this object? value, Type conversion, IFormatProvider? provider = null, DateTimeZoneStrategy dateTimeZone = DateTimeZoneStrategy.Local)
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

        // === bool 的常见字符串变体 ===
        if (targetType == typeof(bool) || targetType == typeof(Boolean))
            return ConvertToBool(value);

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

        // --- DateTime（增强解析：ISO/Z/偏移/Unix 时间戳/自定义格式）---
        if (targetType == typeof(DateTime))
            return ConvertToDateTime(value, provider, dateTimeZone);


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

    private static bool ConvertToBool(object value)
    {
        switch (value)
        {
            case bool b:
                return b;
            case string s:
                var normalized = s.Trim().ToLowerInvariant();
                if (normalized is "true" or "1" or "yes" or "on" or "是" or "对" or "ok")
                    return true;
                if (normalized is "false" or "0" or "no" or "off" or "否" or "错" or "ng")
                    return false;
                break;
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                return Convert.ToInt64(value) != 0;
        }
        throw new InvalidCastException($"无法将类型 {value.GetType()} 转为 bool。");
    }


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


    private static readonly string[] DateTimeFormats =
    {
        // 本地/常见
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy/MM/dd",
        "dd/MM/yyyy HH:mm:ss",
        "dd-MM-yyyy HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fff",
        "yyyy-MM-dd HH:mm:ss.ffffff",
        "yyyy-MM-dd HH:mm:ss.fffffff",

        // ISO 8601 基本
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.fff",
        "yyyy-MM-ddTHH:mm:ss.ffffff",
        "yyyy-MM-ddTHH:mm:ss.fffffff",

        // ISO 8601 带 Z（UTC）
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffffffZ",

        // ISO 8601 带偏移（RFC 3339）
        // 使用 K：匹配 Z 或 +hh:mm/-hh:mm
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.fffK",
        "yyyy-MM-ddTHH:mm:ss.ffffffK",
        "yyyy-MM-ddTHH:mm:ss.fffffffK",
    };

    private const DateTimeStyles StylesExact = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
    private const DateTimeStyles StylesLoose = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    private static DateTime ConvertToDateTime(object value, IFormatProvider? provider, DateTimeZoneStrategy strategy)
    {
        // 直接类型
        if (value is DateTime dt)
            return dt;

        if (value is DateTimeOffset dto)
            return strategy == DateTimeZoneStrategy.Utc ? dto.UtcDateTime : dto.LocalDateTime;

        var culture = provider as CultureInfo ?? CultureInfo.InvariantCulture;

        // 数值类：支持 Unix 时间戳（秒/毫秒）
        switch (value)
        {
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                {
                    var i64 = System.Convert.ToInt64(value, culture);
                    return FromUnix(i64, strategy);
                }
            case float or double or decimal:
                {
                    var dbl = System.Convert.ToDouble(value, culture);
                    return FromUnixDoubleSeconds(dbl, strategy);
                }
            case string s:
                {
                    s = s.Trim();
                    if (string.IsNullOrEmpty(s))
                        throw new FormatException("空字符串无法转换为 DateTime。");

                    // 字符串数字：当做 Unix 时间戳
                    if (IsAllDigits(s))
                    {
                        var i64 = long.Parse(s, CultureInfo.InvariantCulture);
                        return FromUnix(i64, strategy);
                    }

                    // 先尝试 DateTimeOffset 的精确匹配（保留偏移语义）
                    if (DateTimeOffset.TryParseExact(s, DateTimeFormats, CultureInfo.InvariantCulture, StylesExact, out var dto2))
                        return strategy == DateTimeZoneStrategy.Utc ? dto2.UtcDateTime : dto2.LocalDateTime;

                    // 再尝试 DateTime 的精确匹配
                    if (DateTime.TryParseExact(s, DateTimeFormats, CultureInfo.InvariantCulture, StylesExact, out var dt2))
                        return dt2;

                    // 最后宽松解析
                    if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, StylesLoose, out var dto3))
                        return strategy == DateTimeZoneStrategy.Utc ? dto3.UtcDateTime : dto3.LocalDateTime;

                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture, StylesLoose, out var dt3))
                        return dt3;

                    throw new FormatException($"无法解析为 DateTime：\"{s}\"。");
                }
        }

        throw new InvalidCastException($"不支持将类型 {value.GetType()} 转为 DateTime。");
    }

    private static bool IsAllDigits(string s)
    {
        foreach (var c in s)
        {
            if (c < '0' || c > '9') return false;
        }
        return s.Length > 0;
    }

    private static DateTime FromUnix(long v, DateTimeZoneStrategy strategy)
    {
        // 粗略判断毫秒 vs 秒：>= 10^10 认为是毫秒
        var isMillis = v >= 10_000_000_000L;
        var epoch = DateTime.UnixEpoch; // UTC
        var utc = isMillis ? epoch.AddMilliseconds(v) : epoch.AddSeconds(v);
        return strategy == DateTimeZoneStrategy.Utc ? utc : utc.ToLocalTime();
    }

    private static DateTime FromUnixDoubleSeconds(double v, DateTimeZoneStrategy strategy)
    {
        var epoch = DateTime.UnixEpoch; // UTC
        var utc = epoch.AddSeconds(v);
        return strategy == DateTimeZoneStrategy.Utc ? utc : utc.ToLocalTime();
    }

}


public enum DateTimeZoneStrategy
{
    Local, // 返回 LocalDateTime
    Utc    // 返回 UtcDateTime
}
