using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 主题选择器：一排小色块，每块用该主题的窗口底色 + 中心强调色点表示，
/// 选中时套一圈强调色圆环。比下拉框更像 Apple 的外观设置。
/// </summary>
public sealed class ThemeSwatchRow : ThemedControl
{
    private readonly Dictionary<string, double> _hover = new(StringComparer.Ordinal);
    private readonly Dictionary<string, object> _hoverTokens = new(StringComparer.Ordinal);

    private IReadOnlyList<string> _keys = ThemeCatalog.Keys;
    private string _selectedKey = ThemeCatalog.DarkKey;

    public ThemeSwatchRow()
    {
        TabStop = false;
        Height = 40;
        Cursor = Cursors.Hand;
    }

    public event EventHandler? SelectionChanged;

    public IReadOnlyList<string> Keys
    {
        get => _keys;
        set
        {
            _keys = value ?? Array.Empty<string>();
            Invalidate();
        }
    }

    public string SelectedKey
    {
        get => _selectedKey;
        set
        {
            var key = ThemeCatalog.NormalizeKey(value);
            if (string.Equals(key, _selectedKey, StringComparison.Ordinal))
            {
                return;
            }

            _selectedKey = key;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>不触发事件地设置（初始化用）。</summary>
    public void SetSelectedSilently(string key)
    {
        _selectedKey = ThemeCatalog.NormalizeKey(key);
        Invalidate();
    }

    public int SwatchSize { get; set; } = 26;

    public int Gap { get; set; } = 6;

    protected override void OnThemeChanged() => SyncBackdrop();

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        foreach (var (key, rect) in SwatchLayout())
        {
            var isHot = rect.Contains(e.Location);
            var previous = _hover.TryGetValue(key, out var value) ? value : 0d;
            if (isHot != previous > 0.5d)
            {
                Animator.Animate(TokenFor(key), previous, isHot ? 1d : 0d, Motion.Hover, v =>
                {
                    _hover[key] = v;
                    Invalidate();
                }, EaseKind.Standard);
            }
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        foreach (var (key, _) in SwatchLayout())
        {
            var previous = _hover.TryGetValue(key, out var value) ? value : 0d;
            Animator.Animate(TokenFor(key), previous, 0d, Motion.Hover, v =>
            {
                _hover[key] = v;
                Invalidate();
            }, EaseKind.Standard);
        }
    }

    private object TokenFor(string key)
    {
        if (!_hoverTokens.TryGetValue(key, out var token))
        {
            token = new object();
            _hoverTokens[key] = token;
        }

        return token;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        foreach (var (key, rect) in SwatchLayout())
        {
            if (rect.Contains(e.Location))
            {
                SelectedKey = key;
                return;
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        foreach (var (key, rect) in SwatchLayout())
        {
            var theme = ThemeCatalog.Get(key);
            var hover = _hover.TryGetValue(key, out var value) ? value : 0d;
            var isSelected = string.Equals(key, _selectedKey, StringComparison.Ordinal);

            if (hover > 0.01d)
            {
                var haloRect = RectangleF.Inflate(rect, 3f, 3f);
                using var halo = new SolidBrush(Color.FromArgb((int)Math.Round(26 * hover), Theme.Foreground));
                using var haloPath = RoundedPath(haloRect, Radii.Row + 3f);
                graphics.FillPath(halo, haloPath);
            }

            PaintRounded(graphics, rect, Radii.Row, theme.WindowBackground, ThemeCatalog.WithAlpha(Theme.Foreground, 0.22));

            var dotSize = rect.Width * 0.42f;
            var dotRect = new RectangleF(
                rect.X + ((rect.Width - dotSize) / 2f),
                rect.Y + ((rect.Height - dotSize) / 2f),
                dotSize,
                dotSize);
            using (var dotBrush = new SolidBrush(theme.Accent))
            {
                graphics.FillEllipse(dotBrush, dotRect);
            }

            if (isSelected)
            {
                var ringRect = RectangleF.Inflate(rect, 2.5f, 2.5f);
                using var ringPen = new Pen(Theme.Accent, 2f);
                using var ringPath = RoundedPath(ringRect, Radii.Row + 2.5f);
                graphics.DrawPath(ringPen, ringPath);
            }
        }
    }

    private IEnumerable<(string Key, RectangleF Rect)> SwatchLayout()
    {
        var size = SwatchSize;
        var gap = Gap;
        var x = 0f;
        var y = (Height - size) / 2f;

        foreach (var key in _keys)
        {
            if (x + size > Width && x > 0f)
            {
                x = 0f;
                y += size + gap;
            }

            yield return (key, new RectangleF(x, y, size, size));
            x += size + gap;
        }
    }
}
