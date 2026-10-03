using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>Apple 风格开关：胶囊轨道 + 白色圆钮，滑动带缓动。</summary>
public sealed class ToggleSwitch : ThemedControl
{
    private readonly object _valueToken = new();

    private bool _checked;
    private double _position;

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        Size = new Size(42, 25);
        Cursor = Cursors.Hand;
    }

    public event EventHandler? CheckedChanged;

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value)
            {
                return;
            }

            _checked = value;
            AnimateTo(value);
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>不触发事件地设置状态（用于初始化）。</summary>
    public void SetCheckedSilently(bool value)
    {
        _checked = value;
        _position = value ? 1d : 0d;
        Invalidate();
    }

    protected override void OnThemeChanged() => SyncBackdrop();

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            Checked = !Checked;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            Checked = !Checked;
        }
    }

    private void AnimateTo(bool value)
    {
        Animator.Animate(_valueToken, _position, value ? 1d : 0d, Motion.Toggle, position =>
        {
            _position = position;
            Invalidate();
        }, EaseKind.Standard);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var trackHeight = Math.Min(Height, 25);
        var trackWidth = Math.Min(Width, 42);
        var rect = new RectangleF((Width - trackWidth) / 2f, (Height - trackHeight) / 2f, trackWidth, trackHeight);

        var offColor = ThemeCatalog.Over(Theme.Foreground, BackdropColor, Theme.IsDark ? 0.20 : 0.16);
        var trackColor = Animator.Lerp(offColor, Theme.Accent, _position);
        PaintRounded(graphics, rect, trackHeight / 2f, trackColor, null);

        var knobSize = trackHeight - 4f;
        var travel = trackWidth - knobSize - 4f;
        var knobX = rect.X + 2f + (float)(travel * _position);
        var knobRect = new RectangleF(knobX, rect.Y + 2f, knobSize, knobSize);

        using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
        {
            graphics.FillEllipse(shadow, new RectangleF(knobRect.X, knobRect.Y + 1f, knobRect.Width, knobRect.Height));
        }

        using var knobBrush = new SolidBrush(Color.White);
        graphics.FillEllipse(knobBrush, knobRect);

        if (Focused && ShowFocusCues)
        {
            using var focusPen = new Pen(Color.FromArgb(140, Theme.Accent), 1.6f);
            graphics.DrawEllipse(focusPen, RectangleF.Inflate(rect, -1.6f, -1.6f));
        }
    }
}
