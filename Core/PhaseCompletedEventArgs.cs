namespace PotatoClock.Core;

/// <summary>一个阶段结束（自然结束或被跳过）时携带的信息。</summary>
public sealed class PhaseCompletedEventArgs : EventArgs
{
    public PhaseCompletedEventArgs(
        PomodoroPhase completedPhase,
        bool skipped,
        int completedFocusCount,
        PomodoroPhase nextPhase,
        TimeSpan nextDuration)
    {
        CompletedPhase = completedPhase;
        Skipped = skipped;
        CompletedFocusCount = completedFocusCount;
        NextPhase = nextPhase;
        NextDuration = nextDuration;
    }

    /// <summary>刚刚结束的阶段。</summary>
    public PomodoroPhase CompletedPhase { get; }

    /// <summary>是否由「跳过」触发（跳过的专注不计入完成数）。</summary>
    public bool Skipped { get; }

    /// <summary>本次运行内累计完成的专注个数。</summary>
    public int CompletedFocusCount { get; }

    /// <summary>已切换到的下一个阶段。</summary>
    public PomodoroPhase NextPhase { get; }

    /// <summary>下一个阶段的总时长。</summary>
    public TimeSpan NextDuration { get; }
}
