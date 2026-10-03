using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 数值步进控件：左边显示值与单位，右边是 Apple 风格的 − / + 分段步进器。
/// 用来替代 NumericUpDown，避免系统控件无法跟随主题与圆角。
/// </summary>
public sealed class StepperField : ThemedControl
{
    private readonly object _minusHoverToken = new();
    private readonly object _plusHoverToken = new();

    private int _value = 25;
    private int _minimum = 1;
    private int _maximum = 180;
    private double _minusHover;
    private double _plusHover;
    private bool _minusHot;
    private bool _plusHot;

    public StepperField()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        Size = new Size(168, 34);
        Cursor = Cursors.Hand;
    }

    public event EventHandler? ValueChanged;

    /// <summary>单位文案（例如「分钟」）。</summary>
    public string Suffix { get; set; } = string.Empty;

    public int Minimum
    {
        get => _minimum;
        set
        {
            _minimum = Math.Max(0, value);
            Value = Math.Clamp(_value, _minimum, _maximum);
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(_minimum + 1, value);
            Value = Math.Clamp(_value, _minimum, _maximum);
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            if (clamped == _value)
            {
                return;
            }

            _value = clamped;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Step { get; set; } = 1;

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var stepperRect = StepperBounds();
        var track = ThemeCatalog.Over(Theme.Foreground, BackdropColor, Theme.IsDark ? 0.12 : 0.07);
        PaintRounded(graphics, stepperRect, Radii.Stepper, track, null);

        var half = stepperRect.Width / 2f;
        var minusRect = new RectangleF(stepperRect.X, stepperRect.Y, half, stepperRect.Height);
        var plusRect = new RectangleF(stepperRect.X + half, stepperRect.Y, half, stepperRect.Height);

        var minusColor = Color.FromArgb((int)Math.Round(30 * _minusHover), Theme.Foreground);
        var plusColor = Color.FromArgb((int)Math.Round(30 * _plusHover), Theme.Foreground);

        if (minusColor.A > 3)
        {
            var rect = new RectangleF(minusRect.X + 1.5f, minusRect.Y + 1.5f, minusRect.Width - 1.5f, minusRect.Height - 3f);
            PaintRounded(graphics, rect, Radii.Stepper - 1f, minusColor, null);
        }

        if (plusColor.A > 3)
        {
            var rect = new RectangleF(plusRect.X, plusRect.Y + 1.5f, plusRect.Width - 1.5f, plusRect.Height - 3f);
            PaintRounded(graphics, rect, Radii.Stepper - 1f, plusColor, null);
        }

        using (var separatorPen = new Pen(Theme.Separator, 1f))
        {
            graphics.DrawLine(
                separatorPen,
                stepperRect.X + half,
                stepperRect.Y + 6f,
                stepperRect.X + half,
                stepperRect.Bottom - 6f);
        }

        const float glyphSize = 15f;
        Icons.Draw(graphics, IconGlyph.Minus, CenterIn(minusRect, glyphSize), Theme.Foreground);
        Icons.Draw(graphics, IconGlyph.Plus, CenterIn(plusRect, glyphSize), Theme.Foreground);

        // 左侧数值（裁剪到步进器左边的区域，避免长单位压到按钮上）
        var textClip = new RectangleF(0f, 0f, Math.Max(10f, stepperRect.X - 6f), Height);
        var clipState = graphics.Save();
        graphics.SetClip(textClip);

        var valueText = _value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var valueFont = Typography.Get(11.5f, semibold: true);
        var valueSize = Typography.Measure(valueText, valueFont);
        TextRenderer.DrawText(
            graphics,
            valueText,
            valueFont,
            new Rectangle(2, (Height - valueSize.Height) / 2, valueSize.Width + 2, valueSize.Height),
            Theme.Foreground,
            TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        if (!string.IsNullOrEmpty(Suffix))
        {
            var suffixFont = Typography.Get(9.5f);
            var suffixSize = Typography.Measure(Suffix, suffixFont);
            TextRenderer.DrawText(
                graphics,
                Suffix,
                suffixFont,
                new Rectangle(2 + valueSize.Width + 5, (Height - suffixSize.Height) / 2, suffixSize.Width + 2, suffixSize.Height),
                Theme.Secondary,
                TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        graphics.Restore(clipState);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var stepperRect = StepperBounds();
        var half = stepperRect.Width / 2f;
        var isMinus = e.X >= stepperRect.X && e.X < stepperRect.X + half;
        var isPlus = e.X >= stepperRect.X + half && e.X <= stepperRect.Right;

        if (isMinus != _minusHot)
        {
            _minusHot = isMinus;
            AnimateHover(_minusHoverToken, _minusHover, v => _minusHover = v, isMinus ? 1d : 0d);
        }

        if (isPlus != _plusHot)
        {
            _plusHot = isPlus;
            AnimateHover(_plusHoverToken, _plusHover, v => _plusHover = v, isPlus ? 1d : 0d);
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _minusHot = false;
        _plusHot = false;
        AnimateHover(_minusHoverToken, _minusHover, v => _minusHover = v, 0d);
        AnimateHover(_plusHoverToken, _plusHover, v => _plusHover = v, 0d);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var stepperRect = StepperBounds();
        if (e.X < stepperRect.X || e.X > stepperRect.Right || e.Y < stepperRect.Y || e.Y > stepperRect.Bottom)
        {
            return;
        }

        Value += e.X < stepperRect.X + (stepperRect.Width / 2f) ? -Step : Step;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.Right:
                Value += Step;
                break;
            case Keys.Down:
            case Keys.Left:
                Value -= Step;
                break;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Value += e.Delta > 0 ? Step : -Step;
    }

    private void AnimateHover(object token, double from, Action<double> assign, double target) =>
        Animator.Animate(token, from, target, Motion.Hover, value =>
        {
            assign(value);
            Invalidate();
        }, EaseKind.Standard);

    private RectangleF StepperBounds()
    {
        const float stepperWidth = 66f;
        const float stepperHeight = 30f;
        return new RectangleF(Width - stepperWidth, (Height - stepperHeight) / 2f, stepperWidth, stepperHeight);
    }

    private static RectangleF CenterIn(RectangleF bounds, float size) =>
        new(bounds.X + ((bounds.Width - size) / 2f), bounds.Y + ((bounds.Height - size) / 2f), size, size);
}
