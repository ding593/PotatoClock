namespace PotatoClock.App;

/// <summary>
/// 可持久化的应用设置。所有字段都有合理默认值，
/// 反序列化后统一走 <see cref="ClampAndValidate"/> 兜底。
/// </summary>
public sealed class AppSettings
{
    public int Version { get; set; } = 2;

    // ---- 计时 ----
    public int FocusMinutes { get; set; } = 25;

    public int ShortBreakMinutes { get; set; } = 5;

    public int LongBreakMinutes { get; set; } = 15;

    public int RoundsBeforeLongBreak { get; set; } = 4;

    // ---- 提醒 ----
    public bool SoundEnabled { get; set; } = true;

    public bool NotifyEnabled { get; set; } = true;

    // ---- 窗口 ----
    public bool TopMost { get; set; } = true;

    public bool MinimizeToTrayOnClose { get; set; } = true;

    public bool ExpandTasksOnStart { get; set; } = true;

    // ---- 外观 ----
    /// <summary>界面语言：system / zh / en。</summary>
    public string Language { get; set; } = "system";

    /// <summary>主题键：dark / light / warm / mint / violet / contrast / custom。</summary>
    public string ThemeName { get; set; } = ThemeCatalog.DarkKey;

    /// <summary>自定义主题的背景色（#AARRGGBB）。</summary>
    public string BackgroundHex { get; set; } = "#FF1C1C1E";

    /// <summary>自定义主题的文字色（#AARRGGBB）。</summary>
    public string ForegroundHex { get; set; } = "#FFFFFFFF";

    /// <summary>自定义主题的强调色（#AARRGGBB）。</summary>
    public string AccentHex { get; set; } = "#FF0A84FF";

    public static AppSettings CreateDefault() => new();

    public AppSettings Clone() => (AppSettings)MemberwiseClone();

    /// <summary>把数值限制在合法区间，非法颜色与旧主题名回落到默认值。</summary>
    public void ClampAndValidate()
    {
        FocusMinutes = Math.Clamp(FocusMinutes, 1, 180);
        ShortBreakMinutes = Math.Clamp(ShortBreakMinutes, 1, 60);
        LongBreakMinutes = Math.Clamp(LongBreakMinutes, 1, 180);
        RoundsBeforeLongBreak = Math.Clamp(RoundsBeforeLongBreak, 1, 12);

        Language = Loc.ToSettingValue(Loc.Parse(Language));

        var key = ThemeCatalog.NormalizeKey(ThemeName);
        ThemeName = key;

        if (key == ThemeCatalog.CustomKey)
        {
            BackgroundHex = ColorHex.Normalize(BackgroundHex, ColorHex.ToHex(ThemeCatalog.Dark.WindowBackground));
            ForegroundHex = ColorHex.Normalize(ForegroundHex, ColorHex.ToHex(ThemeCatalog.Dark.Foreground));
            AccentHex = ColorHex.Normalize(AccentHex, ColorHex.ToHex(ThemeCatalog.Dark.Accent));
            return;
        }

        // 命中内置主题时让三个颜色字段与该主题保持一致，便于手工编辑 settings.json
        var theme = ThemeCatalog.Get(key);
        BackgroundHex = ColorHex.ToHex(theme.WindowBackground);
        ForegroundHex = ColorHex.ToHex(theme.Foreground);
        AccentHex = ColorHex.ToHex(theme.Accent);
    }
}
