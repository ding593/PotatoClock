using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 任务行：圆形勾选框（勾选动画）+ 标题（完成后加删除线）+ 悬停浮现的删除按钮。
/// </summary>
public sealed class TaskRow : ThemedControl
{
    private readonly object _hoverToken = new();
    private readonly object _checkToken = new();

    private string _title = string.Empty;
    private bool _isDone;
    private bool _isSelected;
    private double _hover;
    private double _check;

    public TaskRow()
    {
        TabStop = false;
        Height = 36;
        Cursor = Cursors.Hand;
    }

    public event EventHandler? ToggleRequested;

    public event EventHandler? DeleteRequested;

    public event EventHandler? Selected;

    /// <summary>任务标题。</summary>
    public string Title
    {
        get => _title;
        set
        {
            _title = value ?? string.Empty;
            Invalidate();
        }
    }

    /// <summary>是否已完成。</summary>
    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value)
            {
                return;
            }

            _isDone = value;
            AnimateCheck();
            Invalidate();
        }
    }

    /// <summary>是否为当前选中行。</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            Invalidate();
        }
    }

    /// <summary>任务标识，便于调用方定位。</summary>
    public Guid TaskId { get; set; }

    protected override void OnThemeChanged() => SyncBackdrop();

    /// <summary>直接设置勾选状态（初始化用，不播动画）。</summary>
    public void SetDoneSilently(bool value)
    {
        _isDone = value;
        _check = value ? 1d : 0d;
        Invalidate();
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
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (DeleteBounds().Contains(e.Location))
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (CheckBounds().Contains(e.Location))
        {
            ToggleRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        Selected?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);

        if (_isSelected)
        {
            PaintRounded(graphics, rect, Radii.Row, Theme.AccentSoft, null);
        }
        else if (_hover > 0.01d)
        {
            PaintRounded(graphics, rect, Radii.Row, Animator.Lerp(BackdropColor, Theme.SurfaceHover, _hover), null);
        }

        // 勾选圆圈
        var checkRect = CheckBounds();
        var checkColor = Animator.Lerp(Theme.Tertiary, Theme.Accent, _check);
        using (var ringPen = new Pen(checkColor, 1.7f))
        {
            graphics.DrawEllipse(ringPen, checkRect);
        }

        if (_check > 0.01d)
        {
            using var fillBrush = new SolidBrush(Theme.Accent);
            graphics.FillEllipse(fillBrush, checkRect);

            var previousClip = graphics.Clip;
            graphics.SetClip(new RectangleF(checkRect.X, checkRect.Y, checkRect.Width * (float)_check, checkRect.Height));
            Icons.Draw(graphics, IconGlyph.Check, RectangleF.Inflate(checkRect, -2.6f, -2.6f), Theme.AccentText, 2.2f);
            graphics.Clip = previousClip;
        }

        // 标题
        var titleFont = Typography.Get(10.5f);
        var textColor = _isDone ? Theme.Tertiary : Theme.Foreground;
        var textRect = new Rectangle(
            (int)checkRect.Right + 10,
            (Height - Typography.Measure("Ag", titleFont).Height) / 2,
            Math.Max(10, (int)DeleteBounds().X - (int)checkRect.Right - 18),
            Typography.Measure("Ag", titleFont).Height + 2);

        TextRenderer.DrawText(
            graphics,
            _title,
            titleFont,
            textRect,
            textColor,
            TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (_isDone)
        {
            var width = Math.Min(Typography.Measure(_title, titleFont).Width, textRect.Width);
            var strikeY = textRect.Y + (textRect.Height / 2);
            using var strikePen = new Pen(Theme.Tertiary, 1f);
            graphics.DrawLine(strikePen, textRect.X, strikeY, textRect.X + width, strikeY);
        }

        // 悬停时的删除按钮
        if (_hover > 0.02d)
        {
            var deleteRect = DeleteBounds();
            var alpha = (int)Math.Round(255 * _hover);
            using var brush = new SolidBrush(Color.FromArgb((int)Math.Round(28 * _hover), Theme.Foreground));
            using var path = RoundedPath(deleteRect, Radii.Row - 1f);
            graphics.FillPath(brush, path);
            Icons.Draw(graphics, IconGlyph.MinusCircle, RectangleF.Inflate(deleteRect, -5f, -5f), Color.FromArgb(alpha, Theme.Secondary));
        }
    }

    private RectangleF CheckBounds() => new(10f, (Height - 20f) / 2f, 20f, 20f);

    private RectangleF DeleteBounds() => new(Width - 32f, (Height - 24f) / 2f, 24f, 24f);

    private void AnimateHover(double target) =>
        Animator.Animate(_hoverToken, _hover, target, Motion.Hover, value =>
        {
            _hover = value;
            Invalidate();
        }, EaseKind.Standard);

    private void AnimateCheck() =>
        Animator.Animate(_checkToken, _check, _isDone ? 1d : 0d, Motion.Toggle, value =>
        {
            _check = value;
            Invalidate();
        }, EaseKind.Standard);
}
