using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>按钮外观。</summary>
public enum ButtonVariant
{
    /// <summary>主操作，强调色填充。</summary>
    Primary,

    /// <summary>次要操作，卡片表面色 + 发丝边。</summary>
    Secondary,

    /// <summary>无底色，仅文字。</summary>
    Ghost,

    /// <summary>纯图标（标题栏按钮）。</summary>
    Icon
}

/// <summary>圆角、带悬停/按下过渡动画的按钮。</summary>
public sealed class FlatButton : ThemedControl
{
    private readonly object _hoverToken = new();
    private readonly object _pressToken = new();

    private ButtonVariant _variant = ButtonVariant.Primary;
    private IconGlyph? _glyph;
    private double _hover;
    private double _press;

    public FlatButton()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        Size = new Size(104, 36);
        Cursor = Cursors.Hand;
        Font = Typography.Get(10.5f, semibold: true);
    }

    public ButtonVariant Variant
    {
        get => _variant;
        set
        {
            _variant = value;
            Invalidate();
        }
    }

    public IconGlyph? Glyph
    {
        get => _glyph;
        set
        {
            _glyph = value;
            Invalidate();
        }
    }

    public float CornerRadius { get; set; } = Radii.Control;

    /// <summary>图标尺寸（px）。</summary>
    public int GlyphSize { get; set; } = 17;

    /// <summary>图标旋转角度（度），用于折叠箭头等过渡动画。</summary>
    public float GlyphRotation { get; set; }

    /// <summary>图标与文字的间距。</summary>
    public int GlyphGap { get; set; } = 7;

    protected override void OnThemeChanged()
    {
        SyncBackdrop();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        AnimateHover(1d);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        AnimateHover(0d);
        AnimatePress(0d);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Focus();
        AnimatePress(1d);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            AnimatePress(0d);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            AnimatePress(1d);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            AnimatePress(0d);
            OnClick(EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var inset = 0.5f;
        var rect = new RectangleF(inset, inset, Width - (inset * 2f), Height - (inset * 2f));
        var color = ResolveFaceColor();

        Color? border = _variant switch
        {
            ButtonVariant.Icon => null,
            ButtonVariant.Primary => null,
            _ => Theme.Separator
        };

        if (_variant == ButtonVariant.Secondary || _variant == ButtonVariant.Ghost)
        {
            PaintRounded(graphics, rect, CornerRadius, color, border);
        }
        else if (_variant == ButtonVariant.Icon)
        {
            if (_hover > 0.01d || _press > 0.01d)
            {
                PaintRounded(graphics, rect, CornerRadius, color, null);
            }
        }
        else
        {
            PaintRounded(graphics, rect, CornerRadius, color, null);
        }

        var content = ResolveContentColor();

        if (Focused && ShowFocusCues)
        {
            using var focusPen = new Pen(Color.FromArgb(150, content), 1.6f);
            var focusRect = RectangleF.Inflate(rect, -2.4f, -2.4f);
            using var path = RoundedPath(focusRect, Math.Max(2f, CornerRadius - 2f));
            graphics.DrawPath(focusPen, path);
        }

        var text = Text ?? string.Empty;
        var hasText = text.Length > 0;
        var textSize = hasText ? Typography.Measure(text, Font) : Size.Empty;
        float totalWidth = textSize.Width;
        float totalHeight = textSize.Height;

        var glyphSize = _glyph is null ? 0 : GlyphSize;
        if (glyphSize > 0)
        {
            totalWidth += glyphSize + (hasText ? GlyphGap : 0);
            totalHeight = Math.Max(totalHeight, glyphSize);
        }

        var startX = (Width - totalWidth) / 2f;
        var contentCenterY = (Height - totalHeight) / 2f;

        if (glyphSize > 0 && _glyph is { } glyph)
        {
            var glyphRect = new RectangleF(startX, (Height - glyphSize) / 2f, glyphSize, glyphSize);
            if (Math.Abs(GlyphRotation) > 0.01f)
            {
                var state = graphics.Save();
                var glyphCenterX = glyphRect.X + (glyphRect.Width / 2f);
                var glyphCenterY = glyphRect.Y + (glyphRect.Height / 2f);
                graphics.TranslateTransform(glyphCenterX, glyphCenterY);
                graphics.RotateTransform(GlyphRotation);
                graphics.TranslateTransform(-glyphCenterX, -glyphCenterY);
                Icons.Draw(graphics, glyph, glyphRect, content);
                graphics.Restore(state);
            }
            else
            {
                Icons.Draw(graphics, glyph, glyphRect, content);
            }

            startX += glyphSize + GlyphGap;
        }

        if (hasText)
        {
            TextRenderer.DrawText(
                graphics,
                text,
                Font,
                new Rectangle((int)Math.Round(startX), (int)Math.Round(contentCenterY), textSize.Width + 2, textSize.Height),
                content,
                TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    private Color ResolveFaceColor()
    {
        var baseColor = _variant switch
        {
            ButtonVariant.Primary => Theme.Accent,
            ButtonVariant.Secondary => Theme.Surface,
            ButtonVariant.Ghost => BackdropColor,
            _ => BackdropColor
        };

        if (_hover <= 0.001d && _press <= 0.001d)
        {
            return baseColor;
        }

        var hoverColor = _variant switch
        {
            ButtonVariant.Primary => Animator.Lerp(Theme.Accent, Color.White, 0.12),
            ButtonVariant.Secondary => Theme.SurfaceHover,
            _ => Theme.SurfaceHover
        };

        var pressColor = _variant switch
        {
            ButtonVariant.Primary => Animator.Lerp(Theme.Accent, Color.Black, 0.12),
            ButtonVariant.Secondary => Theme.SurfacePressed,
            _ => Theme.SurfacePressed
        };

        return Animator.Lerp(Animator.Lerp(baseColor, hoverColor, _hover), pressColor, _press);
    }

    private Color ResolveContentColor()
    {
        if (_variant == ButtonVariant.Primary)
        {
            return Theme.AccentText;
        }

        if (_variant == ButtonVariant.Icon || _variant == ButtonVariant.Ghost)
        {
            return _hover > 0.4d ? Theme.Foreground : Theme.Secondary;
        }

        return Theme.Foreground;
    }

    private void AnimateHover(double target) =>
        Animator.Animate(_hoverToken, _hover, target, Motion.Hover, value =>
        {
            _hover = value;
            Invalidate();
        }, EaseKind.Standard);

    private void AnimatePress(double target) =>
        Animator.Animate(_pressToken, _press, target, Motion.Press, value =>
        {
            _press = value;
            Invalidate();
        }, EaseKind.Decelerate);
}
