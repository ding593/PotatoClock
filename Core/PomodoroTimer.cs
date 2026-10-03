using PotatoClock.App;

namespace PotatoClock.Core;

/// <summary>
/// 番茄钟状态机：只负责阶段、时长与轮次规则，不涉及任何界面。
/// 倒计时以「截止时刻」结算（默认 UtcNow），因此休眠/唤醒后不会丢失阶段，
/// 且不受 UI 定时器抖动影响。
/// </summary>
public sealed class PomodoroTimer
{
    private readonly Func<DateTimeOffset> _clock;

    private int _focusMinutes = 25;
    private int _shortBreakMinutes = 5;
    private int _longBreakMinutes = 15;
    private int? _testSeconds;

    private TimeSpan _remaining;
    private DateTimeOffset _deadlineUtc;
    private bool _isRunning;

    /// <param name="clock">时间源，默认使用 <see cref="DateTimeOffset.UtcNow"/>；注入后便于确定性测试。</param>
    public PomodoroTimer(Func<DateTimeOffset>? clock = null)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        Phase = PomodoroPhase.Focus;
        PhaseDuration = DurationFor(Phase);
        _remaining = PhaseDuration;
    }

    /// <summary>阶段变化或运行状态变化时触发，用于刷新界面。</summary>
    public event EventHandler? StateChanged;

    /// <summary>阶段自然结束或被跳过时触发（切换后）。</summary>
    public event EventHandler<PhaseCompletedEventArgs>? PhaseCompleted;

    public PomodoroPhase Phase { get; private set; }

    /// <summary>当前阶段的总时长。</summary>
    public TimeSpan PhaseDuration { get; private set; }

    public bool IsRunning => _isRunning;

    /// <summary>本次运行内累计完成的专注个数（跳过的不计）。</summary>
    public int CompletedFocusCount { get; private set; }

    /// <summary>每完成几个专注进入一次长休息。</summary>
    public int RoundsBeforeLongBreak { get; private set; } = 4;

    /// <summary>当前专注是这一组里的第几个（1 起）。</summary>
    public int RoundIndex => RoundsBeforeLongBreak > 0
        ? (CompletedFocusCount % RoundsBeforeLongBreak) + 1
        : 1;

    /// <summary>剩余时间；暂停时为冻结值。</summary>
    public TimeSpan Remaining
    {
        get
        {
            var value = _isRunning ? _deadlineUtc - _clock() : _remaining;
            return value < TimeSpan.Zero ? TimeSpan.Zero : value;
        }
    }

    /// <summary>当前阶段已过去的比例（0..1），用于进度条。</summary>
    public double Progress
    {
        get
        {
            if (PhaseDuration <= TimeSpan.Zero)
            {
                return 0d;
            }

            var elapsed = 1d - (Remaining.TotalMilliseconds / PhaseDuration.TotalMilliseconds);
            return Math.Clamp(elapsed, 0d, 1d);
        }
    }

    /// <summary>从当前阶段开始（或暂停后继续）计时。</summary>
    public void StartNext()
    {
        if (_isRunning)
        {
            return;
        }

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = PhaseDuration <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : PhaseDuration;
        }

        _deadlineUtc = _clock() + _remaining;
        _isRunning = true;
        RaiseStateChanged();
    }

    /// <summary>开始 / 暂停切换。</summary>
    public void Toggle()
    {
        if (_isRunning)
        {
            Pause();
        }
        else
        {
            StartNext();
        }
    }

    public void Pause()
    {
        if (!_isRunning)
        {
            return;
        }

        var left = _deadlineUtc - _clock();
        _remaining = left < TimeSpan.Zero ? TimeSpan.Zero : left;
        _isRunning = false;
        RaiseStateChanged();
    }

    /// <summary>当前阶段回到满时长并停止（不计入完成数）。</summary>
    public void Reset()
    {
        _isRunning = false;
        PhaseDuration = DurationFor(Phase);
        _remaining = PhaseDuration;
        RaiseStateChanged();
    }

    /// <summary>跳过当前阶段：推进到下一阶段，但不计入完成数。</summary>
    public void Skip()
    {
        _isRunning = false;
        _remaining = TimeSpan.Zero;
        Advance(skipped: true);
    }

    /// <summary>由界面定时器每 250ms 调用一次，按截止时刻结算。</summary>
    public void Refresh()
    {
        if (!_isRunning)
        {
            return;
        }

        if (_deadlineUtc - _clock() > TimeSpan.Zero)
        {
            return;
        }

        _isRunning = false;
        _remaining = TimeSpan.Zero;
        Advance(skipped: false);
    }

    /// <summary>
    /// 应用设置。当前阶段若尚未开始（或刚 Reset），立即采用新时长；
    /// 否则当前倒计时保持不变，新时长从下一阶段生效。
    /// </summary>
    public void ApplySettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _focusMinutes = settings.FocusMinutes;
        _shortBreakMinutes = settings.ShortBreakMinutes;
        _longBreakMinutes = settings.LongBreakMinutes;
        RoundsBeforeLongBreak = Math.Max(1, settings.RoundsBeforeLongBreak);

        var target = DurationFor(Phase);
        if (!_isRunning && _remaining == PhaseDuration)
        {
            PhaseDuration = target;
            _remaining = target;
        }

        RaiseStateChanged();
    }

    /// <summary>开发参数 <c>--test-seconds</c> 使用：把每个阶段压缩成 N 秒。</summary>
    public void UseShortDurations(int seconds)
    {
        _testSeconds = seconds > 0 ? seconds : null;
        _isRunning = false;
        PhaseDuration = DurationFor(Phase);
        _remaining = PhaseDuration;
        RaiseStateChanged();
    }

    private void Advance(bool skipped)
    {
        var completedPhase = Phase;
        _isRunning = false;

        if (!skipped && completedPhase == PomodoroPhase.Focus)
        {
            CompletedFocusCount++;
        }

        var next = ResolveNextPhase(completedPhase, skipped);
        Phase = next;
        PhaseDuration = DurationFor(next);
        _remaining = PhaseDuration;

        PhaseCompleted?.Invoke(
            this,
            new PhaseCompletedEventArgs(completedPhase, skipped, CompletedFocusCount, next, PhaseDuration));
        RaiseStateChanged();
    }

    private PomodoroPhase ResolveNextPhase(PomodoroPhase completedPhase, bool skipped)
    {
        if (completedPhase != PomodoroPhase.Focus)
        {
            return PomodoroPhase.Focus;
        }

        var reachedLongBreak = !skipped
            && CompletedFocusCount > 0
            && CompletedFocusCount % RoundsBeforeLongBreak == 0;

        return reachedLongBreak ? PomodoroPhase.LongBreak : PomodoroPhase.ShortBreak;
    }

    private TimeSpan DurationFor(PomodoroPhase phase)
    {
        if (_testSeconds is int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        var minutes = phase switch
        {
            PomodoroPhase.Focus => _focusMinutes,
            PomodoroPhase.ShortBreak => _shortBreakMinutes,
            PomodoroPhase.LongBreak => _longBreakMinutes,
            _ => _focusMinutes
        };

        return TimeSpan.FromMinutes(Math.Max(1, minutes));
    }

    private void RaiseStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
}
