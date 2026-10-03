using System.Drawing.Drawing2D;
using PotatoClock.App;
using PotatoClock.Controls;
using PotatoClock.Design;

namespace PotatoClock.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _draft;
    private readonly AppStore _store;

    private AppTheme _theme = ThemeCatalog.Dark;

    public SettingsForm(AppSettings draft, AppStore store)
    {
        _draft = draft ?? throw new ArgumentNullException(nameof(draft));
        _store = store ?? throw new ArgumentNullException(nameof(store));

        InitializeComponent();
        BringInteractiveControlsToFront();

        PopulateControls();

        WireEvents();
        ApplyThemeFromDraft();
        ApplyLocalization();
        UpdateWritableHint();
    }

    /// <summary>点击「确定」后为最终设置；取消时为 null。</summary>
    public AppSettings? Result { get; private set; }

    /// <summary>
    /// 把可交互控件提到最前：WinForms 里后添加的控件在更下层，
    /// 长语言的标签会覆盖到步进器 / 开关上。
    /// </summary>
    private void BringInteractiveControlsToFront()
    {
        foreach (var control in new Control[]
                 {
                     stpFocus, stpShort, stpLong, stpRounds,
                     swSound, swNotify, swTopMost, swTray, swExpand,
                     segLanguage, themeSwatches,
                     wellBackground, wellForeground, wellAccent,
                     btnDefaults, btnCancel, btnOk, btnClose
                 })
        {
            control.BringToFront();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowChrome.ApplyRoundedRegion(this);
        WindowChrome.EnableDropShadow(this);
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

    // ---------------------------------------------------------------- 初始化

    private void PopulateControls()
    {
        stpFocus.Minimum = 1;
        stpFocus.Maximum = 180;
        stpFocus.Value = _draft.FocusMinutes;

        stpShort.Minimum = 1;
        stpShort.Maximum = 60;
        stpShort.Value = _draft.ShortBreakMinutes;

        stpLong.Minimum = 1;
        stpLong.Maximum = 180;
        stpLong.Value = _draft.LongBreakMinutes;

        stpRounds.Minimum = 1;
        stpRounds.Maximum = 12;
        stpRounds.Value = _draft.RoundsBeforeLongBreak;

        swSound.SetCheckedSilently(_draft.SoundEnabled);
        swNotify.SetCheckedSilently(_draft.NotifyEnabled);
        swTopMost.SetCheckedSilently(_draft.TopMost);
        swTray.SetCheckedSilently(_draft.MinimizeToTrayOnClose);
        swExpand.SetCheckedSilently(_draft.ExpandTasksOnStart);

        segLanguage.Items = new[] { Loc.T.LanguageSystem, Loc.T.LanguageChinese, Loc.T.LanguageEnglish };
        segLanguage.SetSelectedSilently(LanguageIndex(Loc.Parse(_draft.Language)));

        themeSwatches.Keys = ThemeCatalog.Keys;
        themeSwatches.SetSelectedSilently(_draft.ThemeName);

        SyncColorWells();
    }

    private static int LanguageIndex(AppLanguage language) => language switch
    {
        AppLanguage.Chinese => 1,
        AppLanguage.English => 2,
        _ => 0
    };

    private static AppLanguage LanguageFromIndex(int index) => index switch
    {
        1 => AppLanguage.Chinese,
        2 => AppLanguage.English,
        _ => AppLanguage.System
    };

    private void WireEvents()
    {
        btnClose.Click += (_, _) => CancelAndClose();
        btnCancel.Click += (_, _) => CancelAndClose();
        btnOk.Click += (_, _) => ApplyAndClose();
        btnDefaults.Click += (_, _) => RestoreDefaults();

        stpFocus.ValueChanged += (_, _) => _draft.FocusMinutes = stpFocus.Value;
        stpShort.ValueChanged += (_, _) => _draft.ShortBreakMinutes = stpShort.Value;
        stpLong.ValueChanged += (_, _) => _draft.LongBreakMinutes = stpLong.Value;
        stpRounds.ValueChanged += (_, _) => _draft.RoundsBeforeLongBreak = stpRounds.Value;

        swSound.CheckedChanged += (_, _) => _draft.SoundEnabled = swSound.Checked;
        swNotify.CheckedChanged += (_, _) => _draft.NotifyEnabled = swNotify.Checked;
        swTopMost.CheckedChanged += (_, _) => _draft.TopMost = swTopMost.Checked;
        swTray.CheckedChanged += (_, _) => _draft.MinimizeToTrayOnClose = swTray.Checked;
        swExpand.CheckedChanged += (_, _) => _draft.ExpandTasksOnStart = swExpand.Checked;

        segLanguage.SelectionChanged += (_, _) =>
        {
            _draft.Language = Loc.ToSettingValue(LanguageFromIndex(segLanguage.SelectedIndex));
            Loc.Apply(LanguageFromIndex(segLanguage.SelectedIndex));
            ApplyLocalization();
        };

        themeSwatches.SelectionChanged += (_, _) =>
        {
            _draft.ThemeName = themeSwatches.SelectedKey;
            ApplyThemeFromDraft();
        };

        wellBackground.Clicked += (_, _) => PickColor(() => _draft.BackgroundHex, hex => _draft.BackgroundHex = hex);
        wellForeground.Clicked += (_, _) => PickColor(() => _draft.ForegroundHex, hex => _draft.ForegroundHex = hex);
        wellAccent.Clicked += (_, _) => PickColor(() => _draft.AccentHex, hex => _draft.AccentHex = hex);

        pnlHeader.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowChrome.BeginDrag(this);
            }
        };

        lblTitle.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowChrome.BeginDrag(this);
            }
        };

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                CancelAndClose();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ApplyAndClose();
            }
        };
    }

    // ---------------------------------------------------------------- 文案与主题

    /// <summary>按草稿里的语言与主题即时刷新整个设置窗口（Apple 式的实时预览）。</summary>
    private void ApplyLocalization()
    {
        Text = Loc.T.SettingsTitle;
        lblTitle.Text = Loc.T.SettingsTitle;

        lblSectionTimer.Text = Loc.T.SectionTimer;
        lblFocus.Text = Loc.T.FocusMinutes;
        lblShort.Text = Loc.T.ShortBreakMinutes;
        lblLong.Text = Loc.T.LongBreakMinutes;
        lblRounds.Text = Loc.T.LongBreakEvery;

        stpFocus.Suffix = Loc.T.MinutesSuffix;
        stpShort.Suffix = Loc.T.MinutesSuffix;
        stpLong.Suffix = Loc.T.MinutesSuffix;
        stpRounds.Suffix = Loc.T.RoundsSuffix;

        lblSectionAlerts.Text = Loc.T.SectionAlerts;
        lblSound.Text = Loc.T.SoundLabel;
        lblNotify.Text = Loc.T.NotifyLabel;

        lblSectionWindow.Text = Loc.T.SectionWindow;
        lblTopMost.Text = Loc.T.TopMostLabel;
        lblTray.Text = Loc.T.TrayOnCloseLabel;
        lblExpand.Text = Loc.T.ExpandOnStartLabel;

        lblSectionAppearance.Text = Loc.T.SectionAppearance;
        lblLanguage.Text = Loc.T.LanguageLabel;
        lblTheme.Text = Loc.T.ThemeLabel;
        lblCustomColors.Text = Loc.T.SectionCustomColors;
        lblCustomHint.Text = Loc.T.CustomHint;

        wellBackground.Label = Loc.T.BackgroundColor;
        wellForeground.Label = Loc.T.ForegroundColor;
        wellAccent.Label = Loc.T.AccentColor;

        btnDefaults.Text = Loc.T.RestoreDefaults;
        btnCancel.Text = Loc.T.Cancel;
        btnOk.Text = Loc.T.Done;
        lblHint.Text = Loc.T.MidPhaseHint;

        segLanguage.Items = new[] { Loc.T.LanguageSystem, Loc.T.LanguageChinese, Loc.T.LanguageEnglish };
        segLanguage.SetSelectedSilently(LanguageIndex(Loc.Parse(_draft.Language)));

        lblThemeName.Text = Loc.T.ThemeName(ThemeCatalog.NormalizeKey(_draft.ThemeName));
        UpdateWritableHint();
        ApplyThemeFromDraft();
    }

    private void ApplyThemeFromDraft()
    {
        _theme = ThemeCatalog.Resolve(_draft);

        BackColor = _theme.WindowBackground;
        ForeColor = _theme.Foreground;
        pnlHeader.BackColor = _theme.WindowBackground;

        lblTitle.BackColor = _theme.WindowBackground;
        lblTitle.ForeColor = _theme.Foreground;
        lblTitle.Font = Typography.Get(14f, semibold: true);

        var sectionFont = Typography.Get(11f, semibold: true);
        var labelFont = Typography.Get(10f);
        var hintFont = Typography.Get(8.5f);

        foreach (var (label, isSection) in new (Label, bool)[]
                 {
                     (lblSectionTimer, true), (lblSectionAlerts, true), (lblSectionWindow, true), (lblSectionAppearance, true),
                     (lblFocus, false), (lblShort, false), (lblLong, false), (lblRounds, false),
                     (lblSound, false), (lblNotify, false), (lblTopMost, false), (lblTray, false), (lblExpand, false),
                     (lblLanguage, false), (lblTheme, false), (lblCustomColors, false)
                 })
        {
            label.BackColor = _theme.Surface;
            label.ForeColor = isSection ? _theme.Foreground : _theme.Secondary;
            label.Font = isSection ? sectionFont : labelFont;
        }

        lblThemeName.BackColor = _theme.Surface;
        lblThemeName.ForeColor = _theme.Foreground;
        lblThemeName.Font = labelFont;

        lblCustomHint.BackColor = _theme.Surface;
        lblCustomHint.ForeColor = _theme.Tertiary;
        lblCustomHint.Font = hintFont;

        lblHint.BackColor = _theme.WindowBackground;
        lblHint.ForeColor = _theme.Secondary;
        lblHint.Font = hintFont;

        lblDataPath.BackColor = _theme.WindowBackground;
        lblDataPath.ForeColor = _store.IsWritable ? _theme.Secondary : Color.FromArgb(255, 214, 74, 66);
        lblDataPath.Font = hintFont;

        // 先给卡片上色，再给卡片内的控件上色，保证子控件按父级表面取色
        foreach (var control in new ThemedControl[]
                 {
                     cardTimer, cardAlerts, cardWindow, cardAppearance,
                     stpFocus, stpShort, stpLong, stpRounds,
                     swSound, swNotify, swTopMost, swTray, swExpand,
                     segLanguage, themeSwatches,
                     wellBackground, wellForeground, wellAccent,
                     btnDefaults, btnCancel, btnOk, btnClose
                 })
        {
            control.Theme = _theme;
        }

        btnDefaults.Font = Typography.Get(10.5f, semibold: true);
        btnCancel.Font = Typography.Get(10.5f, semibold: true);
        btnOk.Font = Typography.Get(10.5f, semibold: true);

        var isCustom = ThemeCatalog.NormalizeKey(_draft.ThemeName) == ThemeCatalog.CustomKey;
        wellBackground.Enabled = isCustom;
        wellForeground.Enabled = isCustom;
        wellAccent.Enabled = isCustom;
        lblCustomHint.Visible = !isCustom;

        SyncColorWells();
        Invalidate();
    }

    private void SyncColorWells()
    {
        var theme = ThemeCatalog.BuildCustom(_draft.BackgroundHex, _draft.ForegroundHex, _draft.AccentHex);
        wellBackground.Value = theme.WindowBackground;
        wellForeground.Value = theme.Foreground;
        wellAccent.Value = theme.Accent;
    }

    private void UpdateWritableHint()
    {
        var path = _store.DataDirectory;
        lblDataPath.Text = _store.IsWritable
            ? Loc.T.DataFolder(path)
            : $"{Loc.T.DataFolder(path)}  {Loc.T.DataFolderReadOnly}";
    }

    // ---------------------------------------------------------------- 交互

    private void PickColor(Func<string> currentHex, Action<string> apply)
    {
        using var dialog = new ColorDialog
        {
            AnyColor = true,
            FullOpen = true,
            Color = ColorHex.ParseOrDefault(currentHex(), _theme.Accent)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        apply(ColorHex.ToHex(dialog.Color));

        // 手工挑色即视为自定义主题
        _draft.ThemeName = ThemeCatalog.CustomKey;
        themeSwatches.SelectedKey = ThemeCatalog.CustomKey;
        lblThemeName.Text = Loc.T.ThemeName(ThemeCatalog.CustomKey);
        ApplyThemeFromDraft();
    }

    private void RestoreDefaults()
    {
        var defaults = AppSettings.CreateDefault();
        _draft.FocusMinutes = defaults.FocusMinutes;
        _draft.ShortBreakMinutes = defaults.ShortBreakMinutes;
        _draft.LongBreakMinutes = defaults.LongBreakMinutes;
        _draft.RoundsBeforeLongBreak = defaults.RoundsBeforeLongBreak;
        _draft.SoundEnabled = defaults.SoundEnabled;
        _draft.NotifyEnabled = defaults.NotifyEnabled;
        _draft.TopMost = defaults.TopMost;
        _draft.MinimizeToTrayOnClose = defaults.MinimizeToTrayOnClose;
        _draft.ExpandTasksOnStart = defaults.ExpandTasksOnStart;
        _draft.Language = defaults.Language;
        _draft.ThemeName = defaults.ThemeName;
        _draft.BackgroundHex = defaults.BackgroundHex;
        _draft.ForegroundHex = defaults.ForegroundHex;
        _draft.AccentHex = defaults.AccentHex;

        Loc.Apply(Loc.Parse(_draft.Language));
        PopulateControls();

        ApplyLocalization();
    }

    private void ApplyAndClose()
    {
        _draft.ClampAndValidate();
        Result = _draft;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelAndClose()
    {
        Result = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
