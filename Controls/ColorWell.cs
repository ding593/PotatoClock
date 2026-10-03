using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>色块按钮：上方是当前颜色，下方是名称，用于自定义配色的三个入口。</summary>
public sealed class ColorWell : ThemedControl
{
    private readonly object _hoverToken = new();

    private Color _value = Color.Gray;
    private string _label = string.Empty;
    private double _hover;

    public ColorWell()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = false;
        Size = new Size(68, 50);
        Cursor = Cursors.Hand;
    }

    public event EventHandler? Clicked;

    /// <summary>展示的颜色。</summary>
    public Color Value
    {
        get => _value;
        set
        {
            _value = value;
            Invalidate();
        }
    }

    /// <summary>下方标签。</summary>
    public string Label
    {
        get => _label;
        set
        {
            _label = value ?? string.Empty;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Animate(1d);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Animate(0d);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && Enabled)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var wellRect = new RectangleF(0.5f, 0.5f, Width - 1f, 26f);
        var border = _hover > 0.01d
            ? Animator.Lerp(Theme.Separator, Theme.Accent, _hover)
            : Theme.Separator;

        PaintRounded(graphics, wellRect, Radii.Row, _value, border, 1f);

        var font = Typography.Get(9f);
        var size = Typography.Measure(_label, font);
        var alpha = Enabled ? 1d : 0.45d;
        var color = Animator.Lerp(BackdropColor, Theme.Secondary, alpha);
        TextRenderer.DrawText(
            graphics,
            _label,
            font,
            new Rectangle(0, 30, Width, 16),
            color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
    }

    private void Animate(double target) =>
        Animator.Animate(_hoverToken, _hover, target, Motion.Hover, value =>
        {
            _hover = value;
            Invalidate();
        }, EaseKind.Decelerate);
}
