using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PotatoClock.Design;

/// <summary>
/// 字体解析与缓存。按 Apple 的层级使用「显示字重 + 正文字重」：
/// Windows 11 优先 Segoe UI Variable 的光学尺寸变体，Windows 10 退回
/// Segoe UI / Segoe UI Semibold / Segoe UI Light，中文由系统字体链接自动补齐
/// （Microsoft YaHei UI）。所有字体实例都缓存复用，避免 GDI 句柄churn。
/// </summary>
public static class Typography
{
    private static readonly Dictionary<string, Font> Cache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> Installed = new(StringComparer.OrdinalIgnoreCase);

    private static bool _initialized;

    /// <summary>正文（标签、输入框）。</summary>
    public static string Body { get; private set; } = "Segoe UI";

    /// <summary>次级正文（说明文字）。</summary>
    public static string Caption { get; private set; } = "Segoe UI";

    /// <summary>强调字重（标题、按钮、数字）。</summary>
    public static string Semibold { get; private set; } = "Segoe UI Semibold";

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            using var collection = new InstalledFontCollection();
            foreach (var family in collection.Families)
            {
                Installed.Add(family.Name);
            }
        }
        catch (Exception)
        {
            // 枚举失败时退回默认族名
        }

        Body = FirstAvailable("Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI", "Tahoma");
        Caption = FirstAvailable("Segoe UI Variable Small", "Segoe UI", Body);
        Semibold = FirstAvailable("Segoe UI Variable Display Semibold", "Segoe UI Variable Display", "Segoe UI Semibold", "Segoe UI Variable Text", "Segoe UI", Body);
    }

    /// <summary>取字体（带缓存）。只使用 Regular / Semibold 两档字重。</summary>
    public static Font Get(float size, bool semibold = false)
    {
        Initialize();

        var family = semibold ? Semibold : Body;
        var key = $"{family}|{size:0.##}";
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        Font font;
        try
        {
            font = new Font(family, size, FontStyle.Regular, GraphicsUnit.Point);
        }
        catch (ArgumentException)
        {
            font = new Font(Body, size, FontStyle.Regular, GraphicsUnit.Point);
        }

        Cache[key] = font;
        return font;
    }

    public static Font Digits(float size) => Get(size, semibold: true);

    /// <summary>测量文字宽度（GDI，与 TextRenderer 一致）。</summary>
    public static Size Measure(string text, Font font) => TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);

    private static string FirstAvailable(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && Installed.Contains(candidate))
            {
                return candidate;
            }
        }

        return "Segoe UI";
    }
}
