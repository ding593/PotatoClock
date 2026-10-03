using System.Drawing;
using System.Drawing.Drawing2D;

namespace PotatoClock.Design;

/// <summary>
/// 圆角半径（px）。按 Apple 的三种圆角体系取固定值：
/// 浮动窗口 12、卡片 10、控件 8、列表行 6；嵌套时内半径 = 外半径 − 内边距。
/// </summary>
public static class Radii
{
    /// <summary>窗口圆角。</summary>
    public const int Window = 12;

    /// <summary>卡片。</summary>
    public const int Card = 10;

    /// <summary>按钮、输入框。</summary>
    public const int Control = 8;

    /// <summary>列表行、小色块。</summary>
    public const int Row = 6;

    /// <summary>步进器等小控件。</summary>
    public const int Stepper = 6;
}

/// <summary>8 点栅格间距。</summary>
public static class Spacing
{
    public const int Xxs = 2;
    public const int Xs = 4;
    public const int Sm = 8;
    public const int Md = 12;
    public const int Lg = 16;
    public const int Xl = 20;
    public const int Xxl = 28;
}

/// <summary>动效时长（ms），取自 Apple 常见的时长区间。</summary>
public static class Motion
{
    /// <summary>按下反馈（指针按下即触发）。</summary>
    public const int Press = 100;

    /// <summary>悬停 / 小幅位移。</summary>
    public const int Hover = 170;

    /// <summary>状态切换（勾选、开关、分段控件）。</summary>
    public const int Toggle = 200;

    /// <summary>数值变化（进度条）。</summary>
    public const int Value = 200;

    /// <summary>展开 / 折叠。</summary>
    public const int Expand = 400;

    /// <summary>窗口淡入等强调过渡。</summary>
    public const int Emphasized = 400;
}

/// <summary>缓动曲线，控制点取自 Apple 常用曲线。</summary>
public enum EaseKind
{
    /// <summary>线性。</summary>
    Linear,

    /// <summary>cubic-bezier(0.4, 0, 0.2, 1)：通用。</summary>
    Standard,

    /// <summary>cubic-bezier(0.25, 1, 0.5, 1)：悬停 / 小幅位移。</summary>
    Decelerate,

    /// <summary>cubic-bezier(0.4, 0, 1, 1)：离开 / 收起。</summary>
    Accelerate,

    /// <summary>cubic-bezier(0.32, 0.72, 0, 1)：展开 / 折叠 / 面板进入。</summary>
    Emphasized
}

public static class Easing
{
    public static double Evaluate(EaseKind kind, double t) => kind switch
    {
        EaseKind.Linear => t,
        EaseKind.Decelerate => CubicBezier(0.25, 1.0, 0.5, 1.0, t),
        EaseKind.Accelerate => CubicBezier(0.4, 0.0, 1.0, 1.0, t),
        EaseKind.Emphasized => CubicBezier(0.32, 0.72, 0.0, 1.0, t),
        _ => CubicBezier(0.4, 0.0, 0.2, 1.0, t)
    };

    /// <summary>求 cubic-bezier(x1,y1,x2,y2) 在 x 处的 y 值（二分法反解参数）。</summary>
    public static double CubicBezier(double x1, double y1, double x2, double y2, double x)
    {
        if (x <= 0d)
        {
            return 0d;
        }

        if (x >= 1d)
        {
            return 1d;
        }

        var low = 0d;
        var high = 1d;
        for (var i = 0; i < 22; i++)
        {
            var mid = (low + high) / 2d;
            if (Curve(x1, x2, mid) < x)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        return Curve(y1, y2, (low + high) / 2d);
    }

    private static double Curve(double p1, double p2, double t)
    {
        var u = 1d - t;
        return (3d * u * u * t * p1) + (3d * u * t * t * p2) + (t * t * t);
    }
}

/// <summary>一个正在播放的动画。</summary>
public sealed class Animation
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    internal Animation(
        object owner,
        double from,
        double to,
        int durationMs,
        EaseKind ease,
        Action<double> apply,
        Action? completed)
    {
        Owner = owner;
        From = from;
        To = to;
        DurationMs = Math.Max(1, durationMs);
        Ease = ease;
        Apply = apply;
        Completed = completed;
    }

    internal object Owner { get; }

    internal double From { get; }

    internal double To { get; }

    internal int DurationMs { get; }

    internal EaseKind Ease { get; }

    internal Action<double> Apply { get; }

    internal Action? Completed { get; }

    internal double Progress()
    {
        var elapsed = (DateTimeOffset.UtcNow - _startedAt).TotalMilliseconds;
        return Math.Clamp(elapsed / DurationMs, 0d, 1d);
    }
}

/// <summary>
/// 统一的动画驱动：一个 60fps 定时器驱动所有属性动画。
/// 控件在 Dispose 时调用 <see cref="Cancel"/> 防止动画泄漏。
/// </summary>
public static class Animator
{
    private static readonly List<Animation> Running = new();
    private static readonly System.Windows.Forms.Timer Ticker = new() { Interval = 15 };
    private static bool _hooked;

    /// <summary>启动一个 0→1 之外的任意区间动画。</summary>
    public static void Animate(
        object owner,
        double from,
        double to,
        int durationMs,
        Action<double> apply,
        EaseKind ease = EaseKind.Standard,
        Action? completed = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(apply);

        Cancel(owner);
        var animation = new Animation(owner, from, to, durationMs, ease, apply, completed);

        lock (Running)
        {
            Running.Add(animation);
            if (!_hooked)
            {
                Ticker.Tick += (_, _) => Tick();
                Ticker.Start();
                _hooked = true;
            }
        }

        apply(from);
    }

    /// <summary>颜色过渡。</summary>
    public static void AnimateColor(
        object owner,
        Color from,
        Color to,
        int durationMs,
        Action<Color> apply,
        EaseKind ease = EaseKind.Standard,
        Action? completed = null)
    {
        Animate(
            owner,
            0d,
            1d,
            durationMs,
            t => apply(Lerp(from, to, t)),
            ease,
            completed);
    }

    /// <summary>取消某个所有者（通常是控件）的所有动画。</summary>
    public static void Cancel(object owner)
    {
        lock (Running)
        {
            Running.RemoveAll(animation => ReferenceEquals(animation.Owner, owner));
        }
    }

    /// <summary>按比例混合两个颜色（含 alpha）。</summary>
    public static Color Lerp(Color from, Color to, double t)
    {
        var ratio = Math.Clamp(t, 0d, 1d);
        return Color.FromArgb(
            (int)Math.Round(from.A + ((to.A - from.A) * ratio)),
            (int)Math.Round(from.R + ((to.R - from.R) * ratio)),
            (int)Math.Round(from.G + ((to.G - from.G) * ratio)),
            (int)Math.Round(from.B + ((to.B - from.B) * ratio)));
    }

    private static void Tick()
    {
        Animation[] snapshot;
        lock (Running)
        {
            snapshot = Running.ToArray();
        }

        foreach (var animation in snapshot)
        {
            var progress = animation.Progress();
            var eased = Easing.Evaluate(animation.Ease, progress);
            animation.Apply(animation.From + ((animation.To - animation.From) * eased));

            if (progress < 1d)
            {
                continue;
            }

            lock (Running)
            {
                Running.Remove(animation);
                if (Running.Count == 0)
                {
                    Ticker.Stop();
                    _hooked = false;
                }
            }

            animation.Completed?.Invoke();
        }
    }
}
