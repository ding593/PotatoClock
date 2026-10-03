using System.Drawing;
using System.Drawing.Drawing2D;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>卡片表面：圆角 + 发丝边，承载设置分区与任务面板。</summary>
public sealed class SurfacePanel : ThemedControl
{
    private float _radius = Radii.Card;
    private bool _withBorder = true;

    public SurfacePanel()
    {
        TabStop = false;
    }

    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    public bool WithBorder
    {
        get => _withBorder;
        set
        {
            _withBorder = value;
            Invalidate();
        }
    }

    protected override void OnThemeChanged() => SyncBackdrop();

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Theme.WindowBackground);

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        PaintRounded(graphics, rect, Radius, Theme.Surface, _withBorder ? Theme.Separator : null);
    }
}
