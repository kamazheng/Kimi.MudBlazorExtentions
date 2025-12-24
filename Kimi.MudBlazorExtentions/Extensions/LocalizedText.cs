using System.Globalization;

namespace Kimi.MudBlazorExtentions.Extensions;

public static class LocalizedText
{
    public static string GetBoolLocalizedText(bool? key)
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return lang switch
        {
            "zh" => key switch
            {
                true => "是",
                false => "否",
                null => "未设置",
            },
            "fr" => key switch
            {
                true => "Vrai",
                false => "Faux",
                null => "Non défini",
            },
            _ => key switch
            {
                true => "Yes",
                false => "No",
                null => "NotSet",
            },
        };
    }
}