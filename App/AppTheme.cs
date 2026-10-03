using System.Drawing;

namespace PotatoClock.App;

/// <summary>
/// 一套界面配色。按 Apple 的层级组织：窗口底色 → 卡片表面 → 三级文字 → 强调色 → 发丝线。
/// 文字层级用「合成到窗口底色」的不透明色，避免 GDI 文本渲染忽略 alpha 的问题。
/// </summary>
public sealed class AppTheme
{
    public string Key { get; init; } = ThemeCatalog.DarkKey;

    public bool IsCustom { get; init; }

    public bool IsDark { get; init; }

    /// <summary>窗口底色。</summary>
    public Color WindowBackground { get; init; }

    /// <summary>卡片表面。</summary>
    public Color Surface { get; init; }

    /// <summary>悬停表面（比表面亮/暗一档）。</summary>
    public Color SurfaceHover { get; init; }

    /// <summary>按下表面。</summary>
    public Color SurfacePressed { get; init; }

    /// <summary>主要文字。</summary>
    public Color Foreground { get; init; }

    /// <summary>次要文字（约 60%）。</summary>
    public Color Secondary { get; init; }

    /// <summary>三级文字（约 30%）。</summary>
    public Color Tertiary { get; init; }

    /// <summary>强调色（全局只用这一个强调色）。</summary>
    public Color Accent { get; init; }

    /// <summary>强调色之上的文字色。</summary>
    public Color AccentText { get; init; }

    /// <summary>强调色的淡底（选中行、胶囊底）。</summary>
    public Color AccentSoft { get; init; }

    /// <summary>发丝分隔线（带 alpha，绘制时与背景混合）。</summary>
    public Color Separator { get; init; }

    /// <summary>阴影色（带 alpha）。</summary>
    public Color Shadow { get; init; }

    /// <summary>
    /// 系统填充色（Apple 的 systemFill），用于进度条轨道、分段控件底。
    /// </summary>
    public Color FillOver(Color backdrop, double scale = 1d)
    {
        var alpha = (IsDark ? 0.36 : 0.20) * Math.Clamp(scale, 0d, 2d);
        return ThemeCatalog.Over(Color.FromArgb(255, 120, 120, 128), backdrop, alpha);
    }
}

/// <summary>内置主题集合、自定义主题派生、旧配置迁移。颜色取 Apple 统一系统色与暗色表面层级。</summary>
public static class ThemeCatalog
{
    public const string DarkKey = "dark";
    public const string LightKey = "light";
    public const string WarmKey = "warm";
    public const string MintKey = "mint";
    public const string VioletKey = "violet";
    public const string ContrastKey = "contrast";
    public const string CustomKey = "custom";

    private static readonly Color DarkBackground = Color.FromArgb(255, 28, 28, 30);
    private static readonly Color LightBackground = Color.FromArgb(255, 242, 242, 247);

    /// <summary>按设置界面展示顺序排列的主题键。</summary>
    public static readonly IReadOnlyList<string> Keys = new[]
    {
        DarkKey, LightKey, WarmKey, MintKey, VioletKey, ContrastKey, CustomKey
    };

    public static readonly AppTheme Dark = new()
    {
        Key = DarkKey,
        IsDark = true,
        WindowBackground = DarkBackground,
        Surface = Color.FromArgb(255, 44, 44, 46),
        SurfaceHover = Over(Color.FromArgb(255, 235, 235, 245), Color.FromArgb(255, 44, 44, 46), 0.07),
        SurfacePressed = Over(Color.FromArgb(255, 235, 235, 245), Color.FromArgb(255, 44, 44, 46), 0.12),
        Foreground = Color.FromArgb(255, 255, 255, 255),
        Secondary = Over(Color.FromArgb(255, 235, 235, 245), DarkBackground, 0.60),
        Tertiary = Over(Color.FromArgb(255, 235, 235, 245), DarkBackground, 0.30),
        Accent = Color.FromArgb(255, 0, 145, 255),
        AccentText = Color.FromArgb(255, 255, 255, 255),
        AccentSoft = Over(Color.FromArgb(255, 0, 145, 255), DarkBackground, 0.16),
        Separator = Color.FromArgb(153, 84, 84, 88),
        Shadow = Color.FromArgb(80, 0, 0, 0)
    };

    public static readonly AppTheme Light = new()
    {
        Key = LightKey,
        IsDark = false,
        WindowBackground = LightBackground,
        Surface = Color.FromArgb(255, 255, 255, 255),
        SurfaceHover = Over(Color.FromArgb(255, 60, 60, 67), Color.FromArgb(255, 255, 255, 255), 0.05),
        SurfacePressed = Over(Color.FromArgb(255, 60, 60, 67), Color.FromArgb(255, 255, 255, 255), 0.09),
        Foreground = Color.FromArgb(255, 0, 0, 0),
        Secondary = Over(Color.FromArgb(255, 60, 60, 67), LightBackground, 0.60),
        Tertiary = Over(Color.FromArgb(255, 60, 60, 67), LightBackground, 0.30),
        Accent = Color.FromArgb(255, 0, 136, 255),
        AccentText = Color.FromArgb(255, 255, 255, 255),
        AccentSoft = Over(Color.FromArgb(255, 0, 136, 255), LightBackground, 0.14),
        Separator = Color.FromArgb(74, 60, 60, 67),
        Shadow = Color.FromArgb(38, 0, 0, 0)
    };

    public static readonly AppTheme Warm = new()
    {
        Key = WarmKey,
        IsDark = true,
        WindowBackground = Color.FromArgb(255, 31, 26, 21),
        Surface = Color.FromArgb(255, 44, 37, 32),
        SurfaceHover = Over(Color.FromArgb(255, 255, 235, 214), Color.FromArgb(255, 44, 37, 32), 0.07),
        SurfacePressed = Over(Color.FromArgb(255, 255, 235, 214), Color.FromArgb(255, 44, 37, 32), 0.12),
        Foreground = Color.FromArgb(255, 255, 248, 240),
        Secondary = Over(Color.FromArgb(255, 255, 248, 240), Color.FromArgb(255, 31, 26, 21), 0.60),
        Tertiary = Over(Color.FromArgb(255, 255, 248, 240), Color.FromArgb(255, 31, 26, 21), 0.30),
        Accent = Color.FromArgb(255, 255, 146, 48),
        AccentText = Color.FromArgb(255, 36, 20, 4),
        AccentSoft = Over(Color.FromArgb(255, 255, 146, 48), Color.FromArgb(255, 31, 26, 21), 0.16),
        Separator = Color.FromArgb(153, 84, 84, 88),
        Shadow = Color.FromArgb(90, 0, 0, 0)
    };

    public static readonly AppTheme Mint = new()
    {
        Key = MintKey,
        IsDark = true,
        WindowBackground = Color.FromArgb(255, 15, 31, 29),
        Surface = Color.FromArgb(255, 26, 44, 42),
        SurfaceHover = Over(Color.FromArgb(255, 226, 255, 250), Color.FromArgb(255, 26, 44, 42), 0.07),
        SurfacePressed = Over(Color.FromArgb(255, 226, 255, 250), Color.FromArgb(255, 26, 44, 42), 0.12),
        Foreground = Color.FromArgb(255, 236, 255, 252),
        Secondary = Over(Color.FromArgb(255, 236, 255, 252), Color.FromArgb(255, 15, 31, 29), 0.60),
        Tertiary = Over(Color.FromArgb(255, 236, 255, 252), Color.FromArgb(255, 15, 31, 29), 0.30),
        Accent = Color.FromArgb(255, 0, 218, 195),
        AccentText = Color.FromArgb(255, 0, 30, 26),
        AccentSoft = Over(Color.FromArgb(255, 0, 218, 195), Color.FromArgb(255, 15, 31, 29), 0.16),
        Separator = Color.FromArgb(153, 84, 84, 88),
        Shadow = Color.FromArgb(90, 0, 0, 0)
    };

    public static readonly AppTheme Violet = new()
    {
        Key = VioletKey,
        IsDark = true,
        WindowBackground = Color.FromArgb(255, 28, 21, 35),
        Surface = Color.FromArgb(255, 41, 30, 49),
        SurfaceHover = Over(Color.FromArgb(255, 247, 235, 255), Color.FromArgb(255, 41, 30, 49), 0.07),
        SurfacePressed = Over(Color.FromArgb(255, 247, 235, 255), Color.FromArgb(255, 41, 30, 49), 0.12),
        Foreground = Color.FromArgb(255, 248, 242, 255),
        Secondary = Over(Color.FromArgb(255, 248, 242, 255), Color.FromArgb(255, 28, 21, 35), 0.60),
        Tertiary = Over(Color.FromArgb(255, 248, 242, 255), Color.FromArgb(255, 28, 21, 35), 0.30),
        Accent = Color.FromArgb(255, 219, 52, 242),
        AccentText = Color.FromArgb(255, 255, 255, 255),
        AccentSoft = Over(Color.FromArgb(255, 219, 52, 242), Color.FromArgb(255, 28, 21, 35), 0.16),
        Separator = Color.FromArgb(153, 84, 84, 88),
        Shadow = Color.FromArgb(90, 0, 0, 0)
    };

    public static readonly AppTheme Contrast = new()
    {
        Key = ContrastKey,
        IsDark = true,
        WindowBackground = Color.FromArgb(255, 0, 0, 0),
        Surface = Color.FromArgb(255, 20, 20, 20),
        SurfaceHover = Over(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 20, 20, 20), 0.10),
        SurfacePressed = Over(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 20, 20, 20), 0.16),
        Foreground = Color.FromArgb(255, 255, 255, 255),
        Secondary = Over(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0), 0.82),
        Tertiary = Over(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0), 0.56),
        Accent = Color.FromArgb(255, 255, 214, 0),
        AccentText = Color.FromArgb(255, 0, 0, 0),
        AccentSoft = Over(Color.FromArgb(255, 255, 214, 0), Color.FromArgb(255, 0, 0, 0), 0.22),
        Separator = Color.FromArgb(160, 255, 255, 255),
        Shadow = Color.FromArgb(140, 0, 0, 0)
    };

    /// <summary>按主题键取主题；未知键回落深色。</summary>
    public static AppTheme Get(string? key) => NormalizeKey(key) switch
    {
        LightKey => Light,
        WarmKey => Warm,
        MintKey => Mint,
        VioletKey => Violet,
        ContrastKey => Contrast,
        _ => Dark
    };

    /// <summary>按设置解析出实际主题（自定义键走三色派生）。</summary>
    public static AppTheme Resolve(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var key = NormalizeKey(settings.ThemeName);
        return key == CustomKey
            ? BuildCustom(settings.BackgroundHex, settings.ForegroundHex, settings.AccentHex)
            : Get(key);
    }

    /// <summary>
    /// 规范化主题键：接受旧版中文名（深色/浅色/暖橙/青绿/紫罗兰/高对比/自定义）
    /// 与英文展示名，便于旧 settings.json 平滑迁移。
    /// </summary>
    public static string NormalizeKey(string? raw)
    {
        var value = raw?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return DarkKey;
        }

        return value.ToLowerInvariant() switch
        {
            "dark" or "深色" => DarkKey,
            "light" or "浅色" => LightKey,
            "warm" or "暖阳" or "暖橙" => WarmKey,
            "mint" or "薄荷" or "青绿" or "teal" => MintKey,
            "violet" or "紫罗兰" => VioletKey,
            "contrast" or "高对比" or "high contrast" => ContrastKey,
            "custom" or "自定义" => CustomKey,
            _ => DarkKey
        };
    }

    /// <summary>由三个颜色派生自定义主题，其余层级自动生成。</summary>
    public static AppTheme BuildCustom(string backgroundHex, string foregroundHex, string accentHex)
    {
        var background = Opaque(ColorHex.ParseOrDefault(backgroundHex, Dark.WindowBackground));
        var foreground = Opaque(ColorHex.ParseOrDefault(foregroundHex, Dark.Foreground));
        var accent = Opaque(ColorHex.ParseOrDefault(accentHex, Dark.Accent));
        var isDark = Luminance(background) < 0.5;

        return new AppTheme
        {
            Key = CustomKey,
            IsCustom = true,
            IsDark = isDark,
            WindowBackground = background,
            Surface = Over(foreground, background, isDark ? 0.08 : 0.55),
            SurfaceHover = Over(foreground, background, isDark ? 0.13 : 0.65),
            SurfacePressed = Over(foreground, background, isDark ? 0.18 : 0.74),
            Foreground = foreground,
            Secondary = Over(foreground, background, 0.60),
            Tertiary = Over(foreground, background, 0.30),
            Accent = accent,
            AccentText = ContrastText(accent),
            AccentSoft = Over(accent, background, isDark ? 0.18 : 0.14),
            Separator = isDark ? Color.FromArgb(153, 84, 84, 88) : Color.FromArgb(74, 60, 60, 67),
            Shadow = isDark ? Color.FromArgb(90, 0, 0, 0) : Color.FromArgb(38, 0, 0, 0)
        };
    }

    /// <summary>把前景色按 alpha 合成到背景上，得到不透明色（GDI 文本渲染需要）。</summary>
    public static Color Over(Color front, Color back, double alpha)
    {
        var ratio = Math.Clamp(alpha, 0d, 1d);
        return Color.FromArgb(
            255,
            (int)Math.Round(back.R + ((front.R - back.R) * ratio)),
            (int)Math.Round(back.G + ((front.G - back.G) * ratio)),
            (int)Math.Round(back.B + ((front.B - back.B) * ratio)));
    }

    public static Color WithAlpha(Color color, double alpha) =>
        Color.FromArgb((int)Math.Round(Math.Clamp(alpha, 0d, 1d) * 255), color.R, color.G, color.B);

    public static Color Opaque(Color color) => color.A == 255 ? color : Color.FromArgb(255, color.R, color.G, color.B);

    public static double Luminance(Color color) => ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255d;

    /// <summary>在给定底色上取可读的黑或白文字。</summary>
    public static Color ContrastText(Color background) =>
        Luminance(background) > 0.58 ? Color.FromArgb(255, 24, 20, 12) : Color.FromArgb(255, 255, 255, 255);
}
