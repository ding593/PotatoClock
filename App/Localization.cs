using System.Globalization;

namespace PotatoClock.App;

/// <summary>界面语言设置。</summary>
public enum AppLanguage
{
    /// <summary>跟随系统。</summary>
    System,

    /// <summary>简体中文。</summary>
    Chinese,

    /// <summary>English.</summary>
    English
}

/// <summary>
/// 界面文案。所有可见文字都从这里取，方便中英切换。
/// 语言名称本身保持原语言写法（Apple 的做法），不做翻译。
/// </summary>
public abstract class UiStrings
{
    public virtual string WindowTitle => "PotatoClock";

    public virtual string TrayText => "PotatoClock";

    // ---- 阶段 ----
    public abstract string PhaseFocus { get; }

    public abstract string PhaseShortBreak { get; }

    public abstract string PhaseLongBreak { get; }

    // ---- 主界面按钮 ----
    public abstract string Start { get; }

    public abstract string Pause { get; }

    public abstract string Reset { get; }

    public abstract string Skip { get; }

    public abstract string Settings { get; }

    public abstract string Tasks { get; }

    public abstract string TaskAddPlaceholder { get; }

    public abstract string TaskDelete { get; }

    public abstract string NoTasks { get; }

    public abstract string TasksCount(int count);

    public abstract string RoundText(int current, int total);

    public abstract string CompletedText(int completed);

    // ---- 托盘 ----
    public abstract string TrayShow { get; }

    public abstract string TrayHide { get; }

    public abstract string TraySettings { get; }

    public abstract string TrayExit { get; }

    public abstract string TrayHintTitle { get; }

    public abstract string TrayHintBody { get; }

    public abstract string TrayTooltip(string time, string phase);

    // ---- 通知 ----
    public abstract string NotifyFocusDone { get; }

    public abstract string NotifyBreakDone { get; }

    public abstract string NotifyShortBreakBody(string duration);

    public abstract string NotifyLongBreakBody(int completed, string duration);

    public abstract string NotifyFocusNextBody(string duration);

    public abstract string Seconds(int count);

    public abstract string Minutes(int count);

    // ---- 设置：分区与字段 ----
    public abstract string SettingsTitle { get; }

    public abstract string SectionTimer { get; }

    public abstract string SectionAlerts { get; }

    public abstract string SectionWindow { get; }

    public abstract string SectionAppearance { get; }

    public abstract string FocusMinutes { get; }

    public abstract string ShortBreakMinutes { get; }

    public abstract string LongBreakMinutes { get; }

    public abstract string LongBreakEvery { get; }

    public abstract string MinutesSuffix { get; }

    public abstract string RoundsSuffix { get; }

    public abstract string SoundLabel { get; }

    public abstract string NotifyLabel { get; }

    public abstract string TopMostLabel { get; }

    public abstract string TrayOnCloseLabel { get; }

    public abstract string ExpandOnStartLabel { get; }

    public abstract string LanguageLabel { get; }

    public abstract string ThemeLabel { get; }

    public abstract string SectionCustomColors { get; }

    public abstract string BackgroundColor { get; }

    public abstract string ForegroundColor { get; }

    public abstract string AccentColor { get; }

    public abstract string CustomHint { get; }

    public abstract string RestoreDefaults { get; }

    public abstract string Done { get; }

    public abstract string Cancel { get; }

    public abstract string MidPhaseHint { get; }

    public abstract string DataFolder(string path);

    public abstract string DataFolderReadOnly { get; }

    // ---- 主题名 ----
    public abstract string ThemeName(string key);

    // ---- 语言名（自身语言写法，不翻译） ----
    public abstract string LanguageSystem { get; }

    public string LanguageChinese => "中文";

    public string LanguageEnglish => "English";

    // ---- 提示与错误 ----
    public abstract string StoreWarningBody { get; }

    public abstract string SingleInstanceBody { get; }

    public abstract string UnhandledErrorBody { get; }

    public abstract string HelpText { get; }
}

public sealed class ChineseStrings : UiStrings
{
    public override string WindowTitle => "PotatoClock 土豆钟";

    public override string PhaseFocus => "专注";

    public override string PhaseShortBreak => "短休息";

    public override string PhaseLongBreak => "长休息";

    public override string Start => "开始";

    public override string Pause => "暂停";

    public override string Reset => "重置";

    public override string Skip => "跳过";

    public override string Settings => "设置";

    public override string Tasks => "任务";

    public override string TaskAddPlaceholder => "添加任务…";

    public override string TaskDelete => "删除";

    public override string NoTasks => "还没有任务，先加一条吧";

    public override string TasksCount(int count) => $"{count} 项";

    public override string RoundText(int current, int total) => $"第 {current}/{total} 个";

    public override string CompletedText(int completed) => $"已完成 {completed} 个";

    public override string TrayShow => "显示窗口";

    public override string TrayHide => "隐藏窗口";

    public override string TraySettings => "设置…";

    public override string TrayExit => "退出";

    public override string TrayHintTitle => "PotatoClock 仍在运行";

    public override string TrayHintBody => "已最小化到系统托盘，右键托盘图标可以退出。";

    public override string TrayTooltip(string time, string phase) => $"PotatoClock {time} · {phase}";

    public override string NotifyFocusDone => "专注结束";

    public override string NotifyBreakDone => "休息结束";

    public override string NotifyShortBreakBody(string duration) => $"休息 {duration}，起来动一动吧。";

    public override string NotifyLongBreakBody(int completed, string duration) => $"已完成 {completed} 个番茄，长休息 {duration}。";

    public override string NotifyFocusNextBody(string duration) => $"开始下一个专注（{duration}）吧。";

    public override string Seconds(int count) => $"{count} 秒";

    public override string Minutes(int count) => $"{count} 分钟";

    public override string LanguageSystem => "跟随系统";

    public override string SettingsTitle => "设置";

    public override string SectionTimer => "计时";

    public override string SectionAlerts => "提醒";

    public override string SectionWindow => "窗口";

    public override string SectionAppearance => "外观";

    public override string FocusMinutes => "专注时长";

    public override string ShortBreakMinutes => "短休息时长";

    public override string LongBreakMinutes => "长休息时长";

    public override string LongBreakEvery => "长休间隔";

    public override string MinutesSuffix => "分钟";

    public override string RoundsSuffix => "个番茄";

    public override string SoundLabel => "结束提示音";

    public override string NotifyLabel => "系统通知";

    public override string TopMostLabel => "窗口置顶";

    public override string TrayOnCloseLabel => "关闭时最小化到托盘";

    public override string ExpandOnStartLabel => "启动时展开任务面板";

    public override string LanguageLabel => "语言";

    public override string ThemeLabel => "主题";

    public override string SectionCustomColors => "自定义颜色";

    public override string BackgroundColor => "背景";

    public override string ForegroundColor => "文字";

    public override string AccentColor => "强调色";

    public override string CustomHint => "选择「自定义」主题后可调";

    public override string RestoreDefaults => "恢复默认";

    public override string Done => "确定";

    public override string Cancel => "取消";

    public override string MidPhaseHint => "计时进行中修改时长，将在下一个阶段生效。";

    public override string DataFolder(string path) => $"数据保存位置：{path}";

    public override string DataFolderReadOnly => "当前目录不可写，修改不会保存。";

    public override string ThemeName(string key) => key switch
    {
        "light" => "浅色",
        "warm" => "暖阳",
        "mint" => "薄荷",
        "violet" => "紫罗兰",
        "contrast" => "高对比",
        "custom" => "自定义",
        _ => "深色"
    };

    public override string StoreWarningBody => "无法写入数据文件，本次修改只保存在内存中。";

    public override string SingleInstanceBody => "PotatoClock 已经在运行，请在系统托盘里查看。";

    public override string UnhandledErrorBody => "程序遇到一个未处理的错误，详情已记录到 exe 目录的 error.log。";

    public override string HelpText => """
        PotatoClock 土豆钟 — 轻量番茄钟

        用法：PotatoClock.exe [选项]

          --test-seconds=N   开发用：把所有阶段时长压缩成 N 秒（1..3600）
          --data-dir=PATH    覆盖数据目录（默认是 exe 所在目录）
          --log=PATH         开发用：把阶段切换记录追加写入该文件
          --language=zh|en   覆盖界面语言（默认跟随系统）
          --help             显示本帮助

        数据文件：settings.json（设置）与 tasks.json（任务清单），都保存在 exe 所在目录。

        快捷键：空格 开始/暂停，Ctrl+N 新建任务，Esc 收起任务面板。
        """;
}

public sealed class EnglishStrings : UiStrings
{
    public override string PhaseFocus => "Focus";

    public override string PhaseShortBreak => "Short Break";

    public override string PhaseLongBreak => "Long Break";

    public override string Start => "Start";

    public override string Pause => "Pause";

    public override string Reset => "Reset";

    public override string Skip => "Skip";

    public override string Settings => "Settings";

    public override string Tasks => "Tasks";

    public override string TaskAddPlaceholder => "Add a task…";

    public override string TaskDelete => "Delete";

    public override string NoTasks => "No tasks yet — add one below";

    public override string TasksCount(int count) => count == 1 ? "1 task" : $"{count} tasks";

    public override string RoundText(int current, int total) => $"Round {current} of {total}";

    public override string CompletedText(int completed) => completed == 1 ? "1 completed" : $"{completed} completed";

    public override string TrayShow => "Show window";

    public override string TrayHide => "Hide window";

    public override string TraySettings => "Settings…";

    public override string TrayExit => "Quit";

    public override string TrayHintTitle => "PotatoClock is still running";

    public override string TrayHintBody => "Minimized to the system tray. Right-click the tray icon to quit.";

    public override string TrayTooltip(string time, string phase) => $"PotatoClock {time} · {phase}";

    public override string NotifyFocusDone => "Focus finished";

    public override string NotifyBreakDone => "Break finished";

    public override string NotifyShortBreakBody(string duration) => $"Take a {duration} break.";

    public override string NotifyLongBreakBody(int completed, string duration) => $"{completed} pomodoros done — enjoy a {duration} long break.";

    public override string NotifyFocusNextBody(string duration) => $"Time for the next focus session ({duration}).";

    public override string Seconds(int count) => $"{count} s";

    public override string Minutes(int count) => $"{count} min";

    public override string LanguageSystem => "System";

    public override string SettingsTitle => "Settings";

    public override string SectionTimer => "Timer";

    public override string SectionAlerts => "Alerts";

    public override string SectionWindow => "Window";

    public override string SectionAppearance => "Appearance";

    public override string FocusMinutes => "Focus length";

    public override string ShortBreakMinutes => "Short break";

    public override string LongBreakMinutes => "Long break";

    public override string LongBreakEvery => "Long break";

    public override string MinutesSuffix => "min";

    public override string RoundsSuffix => "rounds";

    public override string SoundLabel => "Sound";

    public override string NotifyLabel => "Notifications";

    public override string TopMostLabel => "Always on top";

    public override string TrayOnCloseLabel => "Minimize to tray on close";

    public override string ExpandOnStartLabel => "Show tasks on start";

    public override string LanguageLabel => "Language";

    public override string ThemeLabel => "Theme";

    public override string SectionCustomColors => "Custom colors";

    public override string BackgroundColor => "Background";

    public override string ForegroundColor => "Text";

    public override string AccentColor => "Accent";

    public override string CustomHint => "Available with the Custom theme";

    public override string RestoreDefaults => "Reset to defaults";

    public override string Done => "Done";

    public override string Cancel => "Cancel";

    public override string MidPhaseHint => "Duration changes apply from the next phase.";

    public override string DataFolder(string path) => $"Data folder: {path}";

    public override string DataFolderReadOnly => "This folder is not writable — changes won't be saved.";

    public override string ThemeName(string key) => key switch
    {
        "light" => "Light",
        "warm" => "Warm",
        "mint" => "Mint",
        "violet" => "Violet",
        "contrast" => "Contrast",
        "custom" => "Custom",
        _ => "Dark"
    };

    public override string StoreWarningBody => "Could not write the data file; changes are kept in memory only.";

    public override string SingleInstanceBody => "PotatoClock is already running — check the system tray.";

    public override string UnhandledErrorBody => "Something went wrong. Details were written to error.log next to the app.";

    public override string HelpText => """
        PotatoClock — a lightweight pomodoro timer

        Usage: PotatoClock.exe [options]

          --test-seconds=N   dev: compress every phase to N seconds (1..3600)
          --data-dir=PATH    override the data folder (default: next to the exe)
          --log=PATH         dev: append phase transitions to this file
          --language=zh|en   override the UI language (default: system)
          --help             show this help

        Data files: settings.json and tasks.json, both stored next to the exe.

        Shortcuts: Space start/pause, Ctrl+N new task, Esc collapse the task panel.
        """;
}

/// <summary>当前语言与解析。</summary>
public static class Loc
{
    /// <summary>当前文案。</summary>
    public static UiStrings T { get; private set; } = new ChineseStrings();

    /// <summary>当前语言设置（System 表示跟随系统）。</summary>
    public static AppLanguage Language { get; private set; } = AppLanguage.System;

    /// <summary>解析出的实际语言。</summary>
    public static AppLanguage Resolved { get; private set; } = AppLanguage.Chinese;

    public static void Apply(AppLanguage language)
    {
        Language = language;
        Resolved = language == AppLanguage.System ? DetectSystem() : language;
        T = Resolved == AppLanguage.English ? new EnglishStrings() : new ChineseStrings();
    }

    /// <summary>把设置里的字符串解析成枚举（含非法值兜底）。</summary>
    public static AppLanguage Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "zh" or "zh-cn" or "chinese" or "中文" => AppLanguage.Chinese,
        "en" or "en-us" or "english" => AppLanguage.English,
        _ => AppLanguage.System
    };

    public static string ToSettingValue(AppLanguage language) => language switch
    {
        AppLanguage.Chinese => "zh",
        AppLanguage.English => "en",
        _ => "system"
    };

    /// <summary>系统界面语言：中文环境用中文，其余用英文。</summary>
    public static AppLanguage DetectSystem()
    {
        try
        {
            var culture = CultureInfo.CurrentUICulture;
            if (culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                return AppLanguage.Chinese;
            }

            var installed = CultureInfo.InstalledUICulture;
            return installed.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
                ? AppLanguage.Chinese
                : AppLanguage.English;
        }
        catch (Exception)
        {
            return AppLanguage.Chinese;
        }
    }
}
