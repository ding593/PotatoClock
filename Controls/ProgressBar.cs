using System.Drawing;
using System.Drawing.Drawing2D;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>细圆角进度条，数值变化带缓动。</summary>
public sealed class FlatProgressBar : ThemedControl
{
    private readonly object _valueToken = new();

    private double _value;
    private double _rendered;
    private bool _animate = true;

    public FlatProgressBar()
    {
        TabStop = false;
        Size = new Size(240, 6);
    }

    /// <summary>目标进度 0..1。</summary>
    public double Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, 0d, 1d);
            if (Math.Abs(clamped - _value) < 0.0001d)
            {
                return;
            }

            _value = clamped;
            if (!_animate)
            {
                _rendered = clamped;
                Invalidate();
                return;
            }

            Animator.Animate(_valueToken, _rendered, clamped, Motion.Value, v =>
            {
                _rendered = v;
                Invalidate();
            }, EaseKind.Standard);
        }
    }

    /// <summary>是否启用过渡动画（默认开启）。</summary>
    public bool Animate
    {
        get => _animate;
        set => _animate = value;
    }

    /// <summary>立即设置（不播放动画）。</summary>
    public void SnapTo(double value)
    {
        _value = Math.Clamp(value, 0d, 1d);
        _rendered = _value;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var height = Math.Max(3, Height);
        var rect = new RectangleF(0f, 0f, Math.Max(1f, Width), height);
        var radius = height / 2f;

        var track = ThemeCatalog.Over(Theme.Foreground, BackdropColor, Theme.IsDark ? 0.14 : 0.10);
        PaintRounded(graphics, rect, radius, track, null);

        if (_rendered <= 0.001d)
        {
            return;
        }

        var fillWidth = (float)Math.Max(radius * 2f, rect.Width * _rendered);
        var fillRect = new RectangleF(0f, 0f, Math.Min(rect.Width, fillWidth), height);
        PaintRounded(graphics, fillRect, radius, Theme.Accent, null);
    }
}
