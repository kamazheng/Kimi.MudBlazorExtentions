using System.Globalization;
using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TypeExtensions = Kimi.MudBlazorExtentions.Extensions.TypeExtensions;

namespace Kimi.MudBlazorExtentions.Generics;

public partial class GenericInput<T> : MudBaseInput<T>
{

    [Parameter]
    public bool WrapWithMudItem { get; set; } = true;

    [Parameter]
    public bool? OverrideHelperTextOnFocus { get; set; }

    [Parameter]
    public int LabelLevels { get; set; } = 1;

    private Type underlyingType => Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
    private bool isNullable => Nullable.GetUnderlyingType(typeof(T)) != null;
    private System.TypeCode typeCode => Type.GetTypeCode(underlyingType);
    private bool Clearable { get; set; }

    private string title = "";


    private InputType GetInputTypeForText()
    {
        var t = underlyingType;
        if (t == typeof(string)) return InputType.Text;
        if (t == typeof(Guid)) return InputType.Text;
        if (t == typeof(Uri)) return InputType.Url;
        if (t == typeof(DateTime)) return InputType.DateTimeLocal;
        if (t == typeof(TimeSpan)) return InputType.Time;
        return InputType.Text;
    }
    private T? GetSafeDTValue()
    {
        if (Value == null || Value.ToString() == DateTime.MinValue.ToString())
        {
            if (isNullable)
                return default!;
            return TypeExtensions.ChangeType<T>(DateTime.Now)!;
        }
        return Value;
    }

    private void OnValueChanged(T newValue)
    {
        Value = newValue;
        ValueChanged.InvokeAsync(Value);
    }

    protected override void OnInitialized()
    {
        var property = this.For?.GetPropertyInfo();
        if (property is not null)
        {
            Label ??= For?.GetDisplayLabel(LabelLevels) ?? property.GetDisplayLabel();
            title = property.GetXmlSummary() ?? "";
            // HelperTextOnFocus = OverrideHelperTextOnFocus ?? true;
        }
    }


    private MudBlazor.Converter<T, string>? _textConverter;

    private MudBlazor.Converter<T, string>? GetTextConverterForT(bool preferSlashFormat = false)
    {
        if (_textConverter != null) return _textConverter;

        var t = typeof(T);
        var invariant = CultureInfo.InvariantCulture;

        if (t == typeof(DateOnly))
        {
            // 允许解析两种输入：yyyy-MM-dd 与 yyyy/MM/dd
            string[] parseFormats = ["yyyy-MM-dd", "yyyy/MM/dd"];
            string outputFormat = preferSlashFormat ? "yyyy/MM/dd" : "yyyy-MM-dd";

            _textConverter = new MudBlazor.Converter<T, string>
            {
                Culture = invariant,

                // T? -> string?（输出你希望的格式）
                SetFunc = (T? value) =>
                {
                    if (value is null) return string.Empty;
                    var d = (DateOnly)(object)value;
                    return d.ToString(outputFormat, invariant);
                },

                // string? -> T?（解析时兼容两种格式）
                GetFunc = (string? s) =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return default;
                    return DateOnly.TryParseExact(
                        s,
                        parseFormats,
                        invariant,
                        DateTimeStyles.None,
                        out var d
                    ) ? (T?)(object)d : default;
                }
            };
            return _textConverter;
        }

        if (t == typeof(TimeOnly))
        {
            // 输入可能是 HH:mm 或 HH:mm:ss
            string[] parseFormats = ["HH:mm", "HH:mm:ss"];
            string outputFormat = "HH:mm"; // 建议无秒

            _textConverter = new MudBlazor.Converter<T, string>
            {
                Culture = invariant,
                SetFunc = (T? value) =>
                {
                    if (value is null) return string.Empty;
                    var time = (TimeOnly)(object)value;
                    return time.ToString(outputFormat, invariant);
                },
                GetFunc = (string? s) =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return default;
                    return TimeOnly.TryParseExact(
                        s,
                        parseFormats,
                        invariant,
                        DateTimeStyles.None,
                        out var tOnly
                    ) ? (T?)(object)tOnly : default;
                }
            };
            return _textConverter;
        }

        // 其他类型使用默认转换器
        _textConverter = null;
        return _textConverter;

    }
}