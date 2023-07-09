using Teva.Common.Cultures;

namespace VoicerStudio.Application.Infrastructure;

internal static class CultureNormalizer
{
    /// <summary>
    /// On server some locales are displaying as 'Unknown Locale'.
    /// So this hotfix needed to display the correct locales instead.
    /// </summary>
    public static string GetCultureDisplayName(string cultureName) => cultureName.ToLower() switch
    {
        "az-az"          => "Azerbaijani (Azerbaijan)",
        "bs-ba"          => "Bosnian (Bosnia & Herzegovina)",
        "jv-id"          => "Javanese (Indonesia)",
        "sr-rs"          => "Serbian (Serbia)",
        "su-id"          => "Sundanese (Indonesia)",
        "uz-uz"          => "Uzbek (Uzbekistan)",
        "wuu-cn"         => "Shanghainese (China mainland)",
        "yue-cn"         => "Cantonese (China mainland)",
        "zh-cn-henan"    => "Chinese (China mainland, HENAN)",
        "zh-cn-liaoning" => "Chinese (China mainland, LIAONING)",
        "zh-cn-shaanxi"  => "Chinese (China mainland, SHAANXI)",
        "zh-cn-shandong" => "Chinese (China mainland, SHANDONG)",
        "zh-cn-sichuan"  => "Chinese (China mainland, SICHUAN)",
        _                => CultureHelper.GetCulture(cultureName).DisplayName
    };
}