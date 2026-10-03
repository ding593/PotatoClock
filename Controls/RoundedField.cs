using System.Drawing;
using System.Drawing.Drawing2D;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 圆角输入框容器：内部承载一个无边框的原生 TextBox
/// （必须保留原生编辑控件，否则中文输入法无法工作）。
/// </summary>
public sealed class RoundedField : ThemedControl
{
    private readonly object _focusToken = new();

    private double _focus;
    private int _cornerRadius = Radii.Control;

    public RoundedField()
    {
        TabStop = false;

        // 必须先创建内部输入框：设置 Size/Height 会触发 OnResize → LayoutInput
        Input = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = Typography.Get(10.5f)
        };
        Input.GotFocus += (_, _) => AnimateFocus(1d);
        Input.LostFocus += (_, _) => AnimateFocus(0d);
        Input.TextChanged += (_, _) => Invalidate();
        Controls.Add(Input);

        Size = new Size(240, 38);
    }

    /// <summary>内部的原生输入框。</summary>
    public TextBox Input { get; }

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = value;
            Invalidate();
        }
    }

    /// <summary>内边距（左右）。</summary>
    public int Inset { get; set; } = 12;

    protected override void OnThemeChanged()
    {
        SyncBackdrop();

        if (Input is null)
        {
            return;
        }

        Input.BackColor = FieldColor;
        Input.ForeColor = Theme.Foreground;
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutInput();
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Input.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        var border = _focus > 0.01d
            ? Animator.Lerp(Theme.Separator, Theme.Accent, _focus)
            : Theme.Separator;
        PaintRounded(graphics, rect, CornerRadius, FieldColor, border, _focus > 0.01d ? 1.6f : 1f);
    }

    /// <summary>输入框内部的底色（相对所在表面略微抬起/下沉）。</summary>
    public Color FieldColor => ThemeCatalog.Over(Theme.Foreground, BackdropColor, Theme.IsDark ? 0.06 : 0.05);

    private void LayoutInput()
    {
        if (Input is null)
        {
            return;
        }

        var height = Input.PreferredHeight;
        Input.SetBounds(
            Inset,
            Math.Max(0, (Height - height) / 2),
            Math.Max(10, Width - (Inset * 2)),
            height);
    }

    private void AnimateFocus(double target) =>
        Animator.Animate(_focusToken, _focus, target, Motion.Toggle, value =>
        {
            _focus = value;
            Invalidate();
        }, EaseKind.Standard);
}
