using System.Drawing;
using System.Drawing.Drawing2D;
using PotatoClock.Core;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>
/// 阶段进度圆点（类似 Apple 的分页指示器）：
/// 已完成的专注为实心强调色，当前那个是空心圆环，其余为三级色。
/// </summary>
public sealed class PhaseDots : ThemedControl
{
    private int _completed;
    private int _total = 4;
    private bool _breakPhase;
    private double[] _fills = Array.Empty<double>();

    public PhaseDots()
    {
        TabStop = false;
        Size = new Size(120, 10);
    }

    /// <summary>当前阶段。</summary>
    public PomodoroPhase Phase { get; set; } = PomodoroPhase.Focus;

    /// <summary>本组内已完成的专注数（0..Total）。</summary>
    public int Completed
    {
        get => _completed;
        set
        {
            var clamped = Math.Clamp(value, 0, Math.Max(1, _total));
            if (clamped == _completed)
            {
                return;
            }

            _completed = clamped;
            AnimateFills();
        }
    }

    /// <summary>一组几个专注。</summary>
    public int Total
    {
        get => _total;
        set
        {
            var clamped = Math.Clamp(value, 1, 12);
            if (clamped == _total)
            {
                return;
            }

            _total = clamped;
            _fills = new double[_total];
            AnimateFills();
        }
    }

    /// <summary>休息阶段时把整组当作已完成的展示。</summary>
    public bool BreakPhase
    {
        get => _breakPhase;
        set
        {
            _breakPhase = value;
            Invalidate();
        }
    }

    protected override void OnThemeChanged() => SyncBackdrop();

    private void AnimateFills()
    {
        if (_fills.Length != _total)
        {
            _fills = new double[_total];
        }

        for (var i = 0; i < _total; i++)
        {
            var target = i < _completed ? 1d : 0d;
            var index = i;
            Design.Animator.Animate(this, _fills[index], target, Motion.Toggle, value =>
            {
                _fills[index] = value;
                Invalidate();
            }, EaseKind.Standard);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        const float diameter = 8f;
        const float gap = 7f;
        var totalWidth = (_total * diameter) + ((_total - 1) * gap);
        var startX = (Width - totalWidth) / 2f;
        var top = (Height - diameter) / 2f;

        for (var i = 0; i < _total; i++)
        {
            var x = startX + (i * (diameter + gap));
            var rect = new RectangleF(x, top, diameter, diameter);
            var fill = _fills.Length > i ? _fills[i] : 0d;

            if (_breakPhase)
            {
                fill = i < _completed ? 1d : fill;
            }

            using var ringPen = new Pen(Theme.Tertiary, 1.5f);
            graphics.DrawEllipse(ringPen, rect);

            if (fill > 0.001d)
            {
                var scale = 0.6f + (0.4f * (float)fill);
                var filled = new RectangleF(
                    rect.X + ((rect.Width - (rect.Width * scale)) / 2f),
                    rect.Y + ((rect.Height - (rect.Height * scale)) / 2f),
                    rect.Width * scale,
                    rect.Height * scale);
                using var brush = new SolidBrush(Theme.Accent);
                graphics.FillEllipse(brush, filled);
            }
            else if (i == _completed && Phase == PomodoroPhase.Focus)
            {
                using var currentPen = new Pen(Theme.Accent, 1.7f);
                graphics.DrawEllipse(currentPen, rect);
            }
        }
    }
}
