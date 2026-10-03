namespace PotatoClock.Core;

/// <summary>番茄钟阶段。</summary>
public enum PomodoroPhase
{
    /// <summary>专注。</summary>
    Focus,

    /// <summary>短休息。</summary>
    ShortBreak,

    /// <summary>长休息。</summary>
    LongBreak
}

public static class PomodoroPhaseExtensions
{
    /// <summary>阶段的中文显示名。</summary>
    public static string ToDisplayName(this PomodoroPhase phase) => phase switch
    {
        PomodoroPhase.Focus => "专注",
        PomodoroPhase.ShortBreak => "短休息",
        PomodoroPhase.LongBreak => "长休息",
        _ => phase.ToString()
    };
}
