using System.Drawing;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 倒计时显示。按「等宽数字槽」绘制，避免每秒跳动时数字抖动；
/// 阶段结束时会用强调色做一次柔和的淡出闪烁。
/// </summary>
public sealed class TimeDisplay : ThemedControl
{
    private readonly object _flashToken = new();

    private TimeSpan _remaining;
    private Font _font = Typography.Digits(46f);
    private float _flash;

    public TimeDisplay()
    {
        TabStop = false;
        Size = new Size(300, 76);
        _font = Typography.Digits(46f);
    }

    /// <summary>字号（pt）。</summary>
    public float FontSize
    {
        get => _font.Size;
        set
        {
            _font = Typography.Digits(value);
            Invalidate();
        }
    }

    /// <summary>剩余时间。</summary>
    public TimeSpan Remaining
    {
        get => _remaining;
        set
        {
            if (ToSeconds(value) == ToSeconds(_remaining))
            {
                return;
            }

            _remaining = value;
            Invalidate();
        }
    }

    protected override void OnThemeChanged() => SyncBackdrop();

    /// <summary>阶段切换时闪一下强调色。</summary>
    public void Flash()
    {
        _flash = 1f;
        Animator.Animate(_flashToken, 1d, 0d, 520, value =>
        {
            _flash = (float)value;
            Invalidate();
        }, EaseKind.Decelerate);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(BackdropColor);

        var text = BuildText(_remaining);
        var digitWidth = MaxDigitWidth();
        var colonWidth = Typography.Measure(":", _font).Width;

        var totalWidth = 0;
        foreach (var character in text)
        {
            totalWidth += character == ':' ? colonWidth : digitWidth;
        }

        var startX = (Width - totalWidth) / 2f;
        var color = _flash <= 0.001f
            ? Theme.Foreground
            : Animator.Lerp(Theme.Foreground, Theme.Accent, _flash);

        var y = (Height - Typography.Measure("0", _font).Height) / 2;

        foreach (var character in text)
        {
            var slotWidth = character == ':' ? colonWidth : digitWidth;
            var glyph = character.ToString();

            if (character != ':')
            {
                var measured = Typography.Measure(glyph, _font).Width;
                var offset = (slotWidth - measured) / 2;
                TextRenderer.DrawText(
                    graphics,
                    glyph,
                    _font,
                    new Rectangle((int)Math.Round(startX + offset), y, measured + 2, Height),
                    color,
                    TextFormatFlags.NoPadding | TextFormatFlags.Top);
            }
            else
            {
                var measured = Typography.Measure(glyph, _font).Width;
                TextRenderer.DrawText(
                    graphics,
                    glyph,
                    _font,
                    new Rectangle((int)Math.Round(startX), y, measured + 2, Height),
                    color,
                    TextFormatFlags.NoPadding | TextFormatFlags.Top);
            }

            startX += slotWidth;
        }
    }

    private int MaxDigitWidth()
    {
        var width = 0;
        for (var digit = 0; digit <= 9; digit++)
        {
            width = Math.Max(width, Typography.Measure(digit.ToString(), _font).Width);
        }

        return width;
    }

    private static string BuildText(TimeSpan remaining)
    {
        var total = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        var seconds = (int)Math.Ceiling(total.TotalSeconds);
        var value = TimeSpan.FromSeconds(seconds);
        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours}:{value.Minutes:D2}:{value.Seconds:D2}"
            : $"{value.Minutes:D2}:{value.Seconds:D2}";
    }

    private static int ToSeconds(TimeSpan value) => (int)Math.Ceiling(Math.Max(0d, value.TotalSeconds));
}
