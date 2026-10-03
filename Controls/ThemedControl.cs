using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 自绘控件基类：统一双缓冲、主题、背景色与圆角辅助。
/// 每个控件自己绘制背景（不做透明），避免 WinForms 透明背景的各种限制。
/// </summary>
public abstract class ThemedControl : Control
{
    private AppTheme _theme = ThemeCatalog.Dark;
    private bool _onSurface;

    protected ThemedControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.UserPaint
            | ControlStyles.ResizeRedraw,
            true);

        Font = Typography.Get(10f);
    }

    /// <summary>当前主题。</summary>
    public AppTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value ?? ThemeCatalog.Dark;
            BackColor = BackdropColor;
            OnThemeChanged();
            Invalidate();
        }
    }

    /// <summary>
    /// 是否位于卡片表面上。默认会自动判断（见 <see cref="OnCardSurface"/>），
    /// 只有在需要覆盖自动判断时才手工设置。
    /// </summary>
    public bool OnSurface
    {
        get => _onSurface;
        set
        {
            _onSurface = value;
            BackColor = BackdropColor;
            Invalidate();
        }
    }

    /// <summary>
    /// 是否位于卡片（<see cref="SurfacePanel"/>）内部。
    /// 自绘控件会整块填充自己的背景，如果不按父级表面取色，
    /// 卡片上就会出现一圈比卡片更深的「黑边」。
    /// </summary>
    protected bool OnCardSurface
    {
        get
        {
            if (_onSurface)
            {
                return true;
            }

            for (var parent = Parent; parent is not null; parent = parent.Parent)
            {
                if (parent is SurfacePanel)
                {
                    return true;
                }

                if (parent is Form)
                {
                    return false;
                }
            }

            return false;
        }
    }

    /// <summary>本控件所处的背景色。</summary>
    protected Color BackdropColor => OnCardSurface ? Theme.Surface : Theme.WindowBackground;

    protected virtual void OnThemeChanged()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Animator.Cancel(this);
        }

        base.Dispose(disposing);
    }

    /// <summary>圆角矩形路径。</summary>
    protected static GraphicsPath RoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0f || rect.Height <= 0f)
        {
            return path;
        }

        var diameter = Math.Max(0f, Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height)));
        if (diameter <= 0.5f)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>描一个圆角矩形（可选填充）。</summary>
    protected static void PaintRounded(Graphics graphics, RectangleF rect, float radius, Color? fill, Color? border, float borderWidth = 1f)
    {
        using var path = RoundedPath(rect, radius);

        if (fill is { } fillColor && fillColor.A > 0)
        {
            using var brush = new SolidBrush(fillColor);
            graphics.FillPath(brush, path);
        }

        if (border is { } borderColor && borderColor.A > 0)
        {
            using var pen = new Pen(borderColor, borderWidth);
            graphics.DrawPath(pen, path);
        }
    }

    /// <summary>主题变化时把背景刷成对应色。</summary>
    protected void SyncBackdrop()
    {
        BackColor = BackdropColor;
    }
}
