using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PotatoClock.App;

/// <summary>
/// 颜色的十六进制文本表示（#AARRGGBB 或 #RRGGBB）。
/// 设置文件里用文本存颜色，避免 Color 结构体序列化出一堆噪声属性。
/// </summary>
public static partial class ColorHex
{
    [GeneratedRegex("^#?(?:[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
    private static partial Regex ColorPattern();

    public static string ToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    public static bool TryParse(string? text, out Color color)
    {
        color = Color.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();
        if (!ColorPattern().IsMatch(value))
        {
            return false;
        }

        // 支持 #AARRGGBB 与 #RRGGBB（后者 alpha 固定为 255）
        value = value.TrimStart('#');
        int a, r, g, b;
        if (value.Length == 8)
        {
            a = ParseByte(value, 0);
            r = ParseByte(value, 2);
            g = ParseByte(value, 4);
            b = ParseByte(value, 6);
        }
        else
        {
            a = 255;
            r = ParseByte(value, 0);
            g = ParseByte(value, 2);
            b = ParseByte(value, 4);
        }

        color = Color.FromArgb(a, r, g, b);
        return true;
    }

    public static Color ParseOrDefault(string? text, Color fallback) => TryParse(text, out var color) ? color : fallback;

    public static string Normalize(string? text, string fallbackHex) => TryParse(text, out var color) ? ToHex(color) : fallbackHex;

    private static int ParseByte(string value, int index) =>
        int.Parse(value.Substring(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
}
