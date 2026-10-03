using PotatoClock.App;
using PotatoClock.Design;
using PotatoClock.Forms;

namespace PotatoClock;

internal static class Program
{
    private const string MutexName = @"Local\PotatoClock.SingleInstance";

    /// <summary>应用程序主入口点。</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Typography.Initialize();
        Loc.Apply(AppLanguage.System);

        var options = CommandLineOptions.Parse(args);

        if (options.ShowHelp)
        {
            MessageBox.Show(
                Loc.T.HelpText,
                Loc.T.WindowTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var mutex = new Mutex(initiallyOwned: false, MutexName, out _);
        if (IsAnotherInstanceRunning(mutex))
        {
            MessageBox.Show(
                Loc.T.SingleInstanceBody,
                Loc.T.WindowTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportFatalError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportFatalError(e.ExceptionObject as Exception);

        var store = new AppStore(options.DataDirectory);
        var settings = store.LoadSettings();

        // 语言优先取命令行覆盖，其次取设置（system 表示跟随系统）
        Loc.Apply(Loc.Parse(options.LanguageOverride ?? settings.Language));

        Application.Run(new MainForm(store, settings, options.TestSeconds, options.LogPath));
    }

    /// <summary>
    /// 尝试取得单实例互斥体。上一个实例被强杀或崩溃时互斥体会被遗弃，
    /// 这种情况按「未在运行」处理并接管所有权，避免误报。
    /// </summary>
    private static bool IsAnotherInstanceRunning(Mutex mutex)
    {
        try
        {
            return !mutex.WaitOne(TimeSpan.Zero, exitContext: false);
        }
        catch (AbandonedMutexException)
        {
            return false;
        }
    }

    /// <summary>
    /// 兜底：把未处理异常的完整信息写到 exe 目录的 error.log 并给出可读提示，
    /// 而不是弹出 WinForms 默认的「继续/退出」对话框。
    /// </summary>
    private static void ReportFatalError(Exception? exception)
    {
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
            var text = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(logPath, text);
        }
        catch (Exception)
        {
            // 记录失败时不再做任何事，避免二次异常
        }

        MessageBox.Show(
            $"{Loc.T.UnhandledErrorBody}{Environment.NewLine}{Environment.NewLine}{exception?.Message}",
            Loc.T.WindowTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
