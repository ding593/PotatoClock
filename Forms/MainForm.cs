using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using PotatoClock.App;
using PotatoClock.Controls;
using PotatoClock.Core;
using PotatoClock.Design;

namespace PotatoClock.Forms;

public partial class MainForm : Form
{
    private const int WindowWidth = 340;
    private const int CollapsedHeight = 246;
    private const int ExpandedHeight = 522;
    private const int TrayTextMaxLength = 63;
    private const int MaxTaskTitleLength = 200;

    private readonly AppStore _store;
    private readonly PomodoroTimer _timer;
    private readonly List<TaskItem> _tasks;
    private readonly Dictionary<Guid, TaskRow> _rows = new();
    private readonly string? _logPath;

    private readonly object _expandToken = new();
    private readonly object _chevronToken = new();

    private AppSettings _settings;
    private AppTheme _theme = ThemeCatalog.Dark;

    private System.Windows.Forms.Timer _uiTimer = null!;
    private NotifyIcon _tray = null!;
    private ContextMenuStrip _trayMenu = null!;
    private ToolStripMenuItem _miShow = null!;
    private ToolStripMenuItem _miToggle = null!;
    private ToolStripMenuItem _miSkip = null!;
    private ToolStripMenuItem _miSettings = null!;
    private ToolStripMenuItem _miExit = null!;
    private Icon? _trayIcon;
    private SettingsForm? _settingsForm;

    private bool _expanded;
    private bool _reallyExiting;
    private bool _trayHintShown;
    private bool _storeWarningShown;
    private int _lastDisplayedSeconds = -1;

    public MainForm(AppStore store, AppSettings settings, int? testSeconds = null, string? logPath = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logPath = logPath;
        _tasks = store.LoadTasks();

        InitializeComponent();

        _timer = new PomodoroTimer();
        _timer.ApplySettings(_settings);
        if (testSeconds is int seconds)
        {
            _timer.UseShortDurations(seconds);
        }

        _expanded = _settings.ExpandTasksOnStart;
        TopMost = _settings.TopMost;

        BuildTray();
        ApplyTheme();
        ApplyLocalization();
        ApplyExpandedState(animate: false);
        WireEvents();

        _timer.StateChanged += (_, _) => UpdateClockUi(force: true);
        _timer.PhaseCompleted += Timer_PhaseCompleted;

        _uiTimer = new System.Windows.Forms.Timer { Interval = 250 };
        _uiTimer.Tick += (_, _) =>
        {
            _timer.Refresh();
            UpdateClockUi();
        };
        _uiTimer.Start();

        RebuildTaskRows();
        UpdateClockUi(force: true);

        // 首次运行就把默认设置写到安装目录，方便直接编辑 settings.json
        if (!File.Exists(_store.SettingsPath))
        {
            PersistSettings();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        WindowChrome.ApplyRoundedRegion(this);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        WindowChrome.PaintBorder(e.Graphics, this, _theme.Separator);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        var closable = _reallyExiting
            || !_settings.MinimizeToTrayOnClose
            || e.CloseReason != CloseReason.UserClosing;

        if (!closable)
        {
            e.Cancel = true;
            HideToTray();
            if (!_trayHintShown)
            {
                _trayHintShown = true;
                _tray.ShowBalloonTip(4000, Loc.T.TrayHintTitle, Loc.T.TrayHintBody, ToolTipIcon.Info);
            }

            return;
        }

        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _uiTimer?.Stop();
        _uiTimer?.Dispose();

        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.Dispose();
        }

        _trayMenu?.Dispose();
        _trayIcon?.Dispose();

        base.OnFormClosed(e);
    }

    // ---------------------------------------------------------------- 主题与文案

    private void ApplyTheme()
    {
        _theme = ThemeCatalog.Resolve(_settings);

        BackColor = _theme.WindowBackground;
        ForeColor = _theme.Foreground;

        pnlHeader.BackColor = _theme.WindowBackground;

        lblPhase.BackColor = _theme.WindowBackground;
        lblPhase.ForeColor = _theme.Foreground;
        lblPhase.Font = Typography.Get(11.5f, semibold: true);

        lblCaption.BackColor = _theme.WindowBackground;
        lblCaption.ForeColor = _theme.Secondary;
        lblCaption.Font = Typography.Get(9f);

        lblTasks.BackColor = _theme.Surface;
        lblTasks.ForeColor = _theme.Foreground;
        lblTasks.Font = Typography.Get(11f, semibold: true);

        lblTasksCount.BackColor = _theme.Surface;
        lblTasksCount.ForeColor = _theme.Secondary;
        lblTasksCount.Font = Typography.Get(9.5f);

        lblEmpty.BackColor = _theme.Surface;
        lblEmpty.ForeColor = _theme.Tertiary;
        lblEmpty.Font = Typography.Get(10f);

        taskFlow.BackColor = _theme.Surface;

        foreach (var control in new ThemedControl[]
                 {
                     logo, timeDisplay, dots, progress, btnToggle, btnReset, btnSkip,
                     btnSettings, btnTasks, btnClose, btnAdd, card, taskField
                 })
        {
            control.Theme = _theme;
        }

        timeDisplay.FontSize = 47f;
        btnToggle.Font = Typography.Get(11f, semibold: true);
        btnReset.Font = Typography.Get(10.5f, semibold: true);
        btnSkip.Font = Typography.Get(10.5f, semibold: true);

        taskField.Input.Font = Typography.Get(10.5f);

        foreach (var row in _rows.Values)
        {
            row.Theme = _theme;
        }

        StyleTrayMenu();
        Invalidate();
    }

    private void ApplyLocalization()
    {
        Text = Loc.T.WindowTitle;
        lblTasks.Text = Loc.T.Tasks;
        lblEmpty.Text = Loc.T.NoTasks;
        taskField.Input.PlaceholderText = Loc.T.TaskAddPlaceholder;
        btnReset.Text = Loc.T.Reset;
        btnSkip.Text = Loc.T.Skip;

        _miShow.Text = Visible ? Loc.T.TrayHide : Loc.T.TrayShow;
        _miSkip.Text = Loc.T.Skip;
        _miSettings.Text = Loc.T.TraySettings;
        _miExit.Text = Loc.T.TrayExit;

        lblTasksCount.Text = Loc.T.TasksCount(_tasks.Count);
        UpdateClockUi(force: true);
    }

    private void StyleTrayMenu()
    {
        _trayMenu.BackColor = _theme.WindowBackground;
        _trayMenu.ForeColor = _theme.Foreground;
        _trayMenu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(_theme));

        foreach (ToolStripItem item in _trayMenu.Items)
        {
            item.BackColor = _theme.WindowBackground;
            item.ForeColor = _theme.Foreground;
        }
    }

    // ---------------------------------------------------------------- 托盘

    private void BuildTray()
    {
        _trayMenu = new ContextMenuStrip();

        _miShow = new ToolStripMenuItem(Loc.T.TrayShow);
        _miShow.Click += (_, _) => ToggleWindowVisibility();

        _miToggle = new ToolStripMenuItem(Loc.T.Start);
        _miToggle.Click += (_, _) => ToggleTimer();

        _miSkip = new ToolStripMenuItem(Loc.T.Skip);
        _miSkip.Click += (_, _) =>
        {
            _timer.Skip();
            UpdateClockUi(force: true);
        };

        _miSettings = new ToolStripMenuItem(Loc.T.TraySettings);
        _miSettings.Click += (_, _) => OpenSettings();

        _miExit = new ToolStripMenuItem(Loc.T.TrayExit);
        _miExit.Click += (_, _) => ExitApplication();

        _trayMenu.Items.AddRange(new ToolStripItem[]
        {
            _miShow,
            _miToggle,
            _miSkip,
            new ToolStripSeparator(),
            _miSettings,
            _miExit
        });

        _trayIcon = LoadApplicationIcon();
        _tray = new NotifyIcon
        {
            ContextMenuStrip = _trayMenu,
            Icon = _trayIcon ?? SystemIcons.Application,
            Text = Loc.T.TrayText,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => ToggleWindowVisibility();
    }

    private static Icon? LoadApplicationIcon()
    {
        try
        {
            using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("PotatoClock.app.ico");
            return stream is null ? null : new Icon(stream, SystemInformation.SmallIconSize);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or FileLoadException)
        {
            return null;
        }
    }

    private void ToggleWindowVisibility()
    {
        if (Visible)
        {
            HideToTray();
        }
        else
        {
            ShowFromTray();
        }
    }

    private void HideToTray()
    {
        Hide();
        _miShow.Text = Loc.T.TrayShow;
    }

    private void ShowFromTray()
    {
        Show();
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        Activate();
        BringToFront();
        _miShow.Text = Loc.T.TrayHide;
    }

    private void ExitApplication()
    {
        _reallyExiting = true;
        PersistSettings();
        SaveTasks();
        Close();
    }

    // ---------------------------------------------------------------- 事件绑定

    private void WireEvents()
    {
        MouseDown += DragArea_MouseDown;
        pnlHeader.MouseDown += DragArea_MouseDown;
        logo.MouseDown += DragArea_MouseDown;
        lblPhase.MouseDown += DragArea_MouseDown;
        lblCaption.MouseDown += DragArea_MouseDown;

        btnToggle.Click += (_, _) => ToggleTimer();
        btnReset.Click += (_, _) =>
        {
            _timer.Reset();
            UpdateClockUi(force: true);
        };
        btnSkip.Click += (_, _) =>
        {
            _timer.Skip();
            UpdateClockUi(force: true);
        };
        btnSettings.Click += (_, _) => OpenSettings();
        btnTasks.Click += (_, _) => SetExpanded(!_expanded);
        btnClose.Click += (_, _) => Close();
        btnAdd.Click += (_, _) => AddTaskFromInput();

        taskField.Input.KeyDown += Input_KeyDown;

        taskFlow.SizeChanged += (_, _) => ResizeRows();
        KeyDown += MainForm_KeyDown;
    }

    private void ToggleTimer()
    {
        _timer.Toggle();
        UpdateClockUi(force: true);
    }

    // ---------------------------------------------------------------- 展开 / 折叠

    private void SetExpanded(bool expanded, bool animate = true)
    {
        _expanded = expanded;

        if (animate)
        {
            Animator.Animate(_chevronToken, btnTasks.GlyphRotation, expanded ? 180f : 0f, Motion.Expand, angle =>
            {
                btnTasks.GlyphRotation = (float)angle;
                btnTasks.Invalidate();
            }, EaseKind.Emphasized);
        }
        else
        {
            btnTasks.GlyphRotation = expanded ? 180f : 0f;
        }

        if (expanded)
        {
            card.Visible = true;
        }

        var target = expanded ? ExpandedHeight : CollapsedHeight;
        if (!animate || ClientSize.Height == target)
        {
            ApplyHeight(target);
            card.Visible = expanded;
            return;
        }

        Animator.Animate(_expandToken, ClientSize.Height, target, Motion.Expand, value =>
        {
            ApplyHeight((int)Math.Round(value));
        }, EaseKind.Emphasized, () =>
        {
            card.Visible = _expanded;
            ResizeRows();
        });
    }

    private void ApplyExpandedState(bool animate)
    {
        if (!_expanded)
        {
            card.Visible = false;
            btnTasks.GlyphRotation = 0f;
            ApplyHeight(CollapsedHeight);
            return;
        }

        SetExpanded(true, animate);
    }

    private void ApplyHeight(int height)
    {
        var working = Screen.FromControl(this).WorkingArea;
        if (Top + height > working.Bottom)
        {
            Top = Math.Max(working.Top, working.Bottom - height);
        }

        ClientSize = new Size(WindowWidth, height);
        WindowChrome.ApplyRoundedRegion(this);
    }

    // ---------------------------------------------------------------- 界面刷新

    private void UpdateClockUi(bool force = false)
    {
        var remaining = _timer.Remaining;
        var displayedSeconds = (int)Math.Ceiling(remaining.TotalSeconds);

        if (!force && displayedSeconds == _lastDisplayedSeconds)
        {
            progress.Value = _timer.Progress;
            return;
        }

        _lastDisplayedSeconds = displayedSeconds;

        timeDisplay.Remaining = remaining;
        progress.Value = _timer.Progress;

        lblPhase.Text = PhaseName(_timer.Phase);
        lblCaption.Text = _timer.Phase == PomodoroPhase.Focus
            ? Loc.T.RoundText(_timer.RoundIndex, _timer.RoundsBeforeLongBreak)
            : Loc.T.CompletedText(_timer.CompletedFocusCount);

        dots.Total = _timer.RoundsBeforeLongBreak;
        dots.Phase = _timer.Phase;
        dots.BreakPhase = _timer.Phase != PomodoroPhase.Focus;
        dots.Completed = _timer.CompletedFocusCount % Math.Max(1, _timer.RoundsBeforeLongBreak);

        var running = _timer.IsRunning;
        btnToggle.Text = running ? Loc.T.Pause : Loc.T.Start;
        btnToggle.Glyph = running ? IconGlyph.Pause : IconGlyph.Play;
        btnToggle.Invalidate();

        _miToggle.Text = btnToggle.Text;

        if (_tray is not null)
        {
            var tooltip = Loc.T.TrayTooltip(FormatTime(remaining), PhaseName(_timer.Phase));
            _tray.Text = tooltip.Length <= TrayTextMaxLength ? tooltip : tooltip[..(TrayTextMaxLength - 1)] + "…";
        }
    }

    private static string PhaseName(PomodoroPhase phase) => phase switch
    {
        PomodoroPhase.ShortBreak => Loc.T.PhaseShortBreak,
        PomodoroPhase.LongBreak => Loc.T.PhaseLongBreak,
        _ => Loc.T.PhaseFocus
    };

    private static string FormatTime(TimeSpan remaining)
    {
        var total = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        var value = TimeSpan.FromSeconds((int)Math.Ceiling(total.TotalSeconds));
        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours}:{value.Minutes:D2}:{value.Seconds:D2}"
            : $"{value.Minutes:D2}:{value.Seconds:D2}";
    }

    // ---------------------------------------------------------------- 阶段结束

    private void Timer_PhaseCompleted(object? sender, PhaseCompletedEventArgs e)
    {
        WriteLog(e);

        timeDisplay.Flash();

        if (!e.Skipped)
        {
            if (_settings.SoundEnabled)
            {
                PlayPhaseSound(e.CompletedPhase);
            }

            if (_settings.NotifyEnabled)
            {
                ShowPhaseNotification(e);
            }
        }

        UpdateClockUi(force: true);
    }

    private static void PlayPhaseSound(PomodoroPhase completedPhase)
    {
        try
        {
            if (completedPhase == PomodoroPhase.Focus)
            {
                SystemSounds.Asterisk.Play();
            }
            else
            {
                SystemSounds.Exclamation.Play();
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or PlatformNotSupportedException)
        {
            // 没有可用音频设备时忽略提示音
        }
    }

    private void ShowPhaseNotification(PhaseCompletedEventArgs e)
    {
        string title;
        string text;

        if (e.CompletedPhase == PomodoroPhase.Focus)
        {
            title = Loc.T.NotifyFocusDone;
            text = e.NextPhase == PomodoroPhase.LongBreak
                ? Loc.T.NotifyLongBreakBody(e.CompletedFocusCount, FormatDuration(e.NextDuration))
                : Loc.T.NotifyShortBreakBody(FormatDuration(e.NextDuration));
        }
        else
        {
            title = Loc.T.NotifyBreakDone;
            text = Loc.T.NotifyFocusNextBody(FormatDuration(e.NextDuration));
        }

        _tray.ShowBalloonTip(5000, title, text, ToolTipIcon.Info);
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalSeconds < 60
            ? Loc.T.Seconds(Math.Max(1, (int)Math.Round(duration.TotalSeconds)))
            : Loc.T.Minutes((int)Math.Round(duration.TotalMinutes));

    private void WriteLog(PhaseCompletedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_logPath))
        {
            return;
        }

        try
        {
            var line = string.Join(
                '\t',
                DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                $"completed={e.CompletedPhase}",
                $"skipped={e.Skipped}",
                $"focusCount={e.CompletedFocusCount}",
                $"next={e.NextPhase}",
                $"nextDurationSeconds={(int)e.NextDuration.TotalSeconds}");
            File.AppendAllText(_logPath!, line + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 日志仅用于开发验证，失败不影响使用
        }
    }

    // ---------------------------------------------------------------- 任务清单

    private void RebuildTaskRows()
    {
        taskFlow.SuspendLayout();
        foreach (var row in _rows.Values)
        {
            row.Dispose();
        }

        _rows.Clear();
        taskFlow.Controls.Clear();

        foreach (var task in _tasks)
        {
            var row = CreateRow(task);
            taskFlow.Controls.Add(row);
            _rows[task.Id] = row;
        }

        taskFlow.ResumeLayout();
        ResizeRows();
        UpdateTaskSummary();
    }

    private TaskRow CreateRow(TaskItem task)
    {
        var row = new TaskRow
        {
            TaskId = task.Id,
            Title = task.Title,
            Theme = _theme,
            OnSurface = true,
            Height = 32,
            Width = RowWidth(),
            Margin = new Padding(0, 0, 0, 2)
        };

        row.SetDoneSilently(task.IsDone);
        row.ToggleRequested += (_, _) => ToggleTask(task.Id);
        row.DeleteRequested += (_, _) => DeleteTask(task.Id);
        return row;
    }

    private int RowWidth() =>
        Math.Max(
            80,
            taskFlow.ClientSize.Width
            - (taskFlow.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0)
            - 4);

    private void ResizeRows()
    {
        var width = RowWidth();
        foreach (Control control in taskFlow.Controls)
        {
            control.Width = width;
        }
    }

    private void UpdateTaskSummary()
    {
        lblEmpty.Visible = _tasks.Count == 0;
        lblTasksCount.Text = Loc.T.TasksCount(_tasks.Count);
    }

    private void ToggleTask(Guid id)
    {
        var task = _tasks.FirstOrDefault(item => item.Id == id);
        if (task is null)
        {
            return;
        }

        task.IsDone = !task.IsDone;
        task.CompletedUtc = task.IsDone ? DateTimeOffset.UtcNow : null;
        SaveTasks();

        if (_rows.TryGetValue(id, out var row))
        {
            row.IsDone = task.IsDone;
        }
    }

    private void DeleteTask(Guid id)
    {
        var task = _tasks.FirstOrDefault(item => item.Id == id);
        if (task is null)
        {
            return;
        }

        _tasks.Remove(task);
        SaveTasks();

        if (_rows.Remove(id, out var row))
        {
            taskFlow.Controls.Remove(row);
            row.Dispose();
        }

        ResizeRows();
        UpdateTaskSummary();
    }

    private void AddTaskFromInput()
    {
        var title = (taskField.Input.Text ?? string.Empty).Trim();
        if (title.Length == 0)
        {
            return;
        }

        if (title.Length > MaxTaskTitleLength)
        {
            title = title[..MaxTaskTitleLength];
        }

        var task = new TaskItem { Title = title };
        _tasks.Add(task);
        SaveTasks();

        taskField.Input.Clear();

        var row = CreateRow(task);
        taskFlow.Controls.Add(row);
        _rows[task.Id] = row;

        ResizeRows();
        UpdateTaskSummary();
        taskFlow.ScrollControlIntoView(row);
        taskField.Input.Focus();
    }

    private void Input_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
        AddTaskFromInput();
    }

    private void SaveTasks()
    {
        if (!_store.SaveTasks(_tasks))
        {
            WarnStoreFailure();
        }
    }

    // ---------------------------------------------------------------- 设置

    private void OpenSettings()
    {
        if (!Visible)
        {
            ShowFromTray();
        }

        if (_settingsForm is { IsDisposed: false } existing)
        {
            if (!existing.Visible)
            {
                existing.Show(this);
                FadeIn(existing);
            }

            existing.Activate();
            return;
        }

        _settingsForm = new SettingsForm(_settings.Clone(), _store);
        _settingsForm.FormClosed += (_, _) => ApplySettingsFromDialog();
        _settingsForm.Show(this);
        PositionSettingsForm(_settingsForm);
        FadeIn(_settingsForm);
    }

    private static void FadeIn(Form form)
    {
        form.Opacity = 0d;
        Animator.Animate(form, 0d, 1d, Motion.Emphasized, value => form.Opacity = value, EaseKind.Decelerate);
    }

    /// <summary>把设置窗口摆在主窗口中间，并保证不越出当前屏幕工作区。</summary>
    private void PositionSettingsForm(Form dialog)
    {
        var ownerBounds = RectangleToScreen(ClientRectangle);
        var workingArea = Screen.FromRectangle(ownerBounds).WorkingArea;

        var x = ownerBounds.Left + ((ownerBounds.Width - dialog.Width) / 2);
        var y = ownerBounds.Top + ((ownerBounds.Height - dialog.Height) / 2);

        x = Math.Clamp(x, workingArea.Left, Math.Max(workingArea.Left, workingArea.Right - dialog.Width));
        y = Math.Clamp(y, workingArea.Top, Math.Max(workingArea.Top, workingArea.Bottom - dialog.Height));

        dialog.Location = new Point(x, y);
    }

    /// <summary>设置窗口关闭后：确定则应用并落盘，取消则丢弃草稿。</summary>
    private void ApplySettingsFromDialog()
    {
        var result = _settingsForm?.Result;
        _settingsForm = null;

        if (result is null)
        {
            return;
        }

        _settings = result;
        TopMost = _settings.TopMost;
        Loc.Apply(Loc.Parse(_settings.Language));
        _timer.ApplySettings(_settings);
        ApplyTheme();
        ApplyLocalization();
        PersistSettings();
        UpdateClockUi(force: true);
    }

    private void PersistSettings()
    {
        if (!_store.SaveSettings(_settings))
        {
            WarnStoreFailure();
        }
    }

    private void WarnStoreFailure()
    {
        if (_storeWarningShown)
        {
            return;
        }

        _storeWarningShown = true;
        var message = $"{Loc.T.StoreWarningBody}\n\n{_store.LastError}";
        MessageBox.Show(
            Visible ? this : null,
            message,
            Loc.T.WindowTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    // ---------------------------------------------------------------- 键盘与拖动

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && !taskField.Input.Focused)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            ToggleTimer();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            SetExpanded(false);
        }
        else if (e.Control && e.KeyCode == Keys.N)
        {
            e.Handled = true;
            SetExpanded(true);
            taskField.Input.Focus();
        }
    }

    private void DragArea_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        WindowChrome.BeginDrag(this);
    }

    /// <summary>让托盘右键菜单跟随主题。</summary>
    private sealed class ThemeColorTable : ProfessionalColorTable
    {
        private readonly AppTheme _theme;

        public ThemeColorTable(AppTheme theme)
        {
            _theme = theme;
            UseSystemColors = false;
        }

        public override Color MenuItemSelected => ThemeCatalog.Over(_theme.WindowBackground, _theme.Foreground, 0.18);

        public override Color MenuItemSelectedGradientBegin => MenuItemSelected;

        public override Color MenuItemSelectedGradientEnd => MenuItemSelected;

        public override Color MenuItemBorder => _theme.Separator;

        public override Color MenuBorder => _theme.Separator;

        public override Color ToolStripDropDownBackground => _theme.Surface;

        public override Color ImageMarginGradientBegin => _theme.Surface;

        public override Color ImageMarginGradientMiddle => _theme.Surface;

        public override Color ImageMarginGradientEnd => _theme.Surface;

        public override Color SeparatorDark => _theme.Separator;

        public override Color SeparatorLight => _theme.Separator;
    }
}
