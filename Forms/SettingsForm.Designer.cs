using PotatoClock.Controls;

namespace PotatoClock.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null!;

        private Panel pnlHeader = null!;
        private Label lblTitle = null!;
        private FlatButton btnClose = null!;

        private SurfacePanel cardTimer = null!;
        private Label lblSectionTimer = null!;
        private Label lblFocus = null!;
        private StepperField stpFocus = null!;
        private Label lblShort = null!;
        private StepperField stpShort = null!;
        private Label lblLong = null!;
        private StepperField stpLong = null!;
        private Label lblRounds = null!;
        private StepperField stpRounds = null!;

        private SurfacePanel cardAlerts = null!;
        private Label lblSectionAlerts = null!;
        private Label lblSound = null!;
        private ToggleSwitch swSound = null!;
        private Label lblNotify = null!;
        private ToggleSwitch swNotify = null!;

        private SurfacePanel cardWindow = null!;
        private Label lblSectionWindow = null!;
        private Label lblTopMost = null!;
        private ToggleSwitch swTopMost = null!;
        private Label lblTray = null!;
        private ToggleSwitch swTray = null!;
        private Label lblExpand = null!;
        private ToggleSwitch swExpand = null!;

        private SurfacePanel cardAppearance = null!;
        private Label lblSectionAppearance = null!;
        private Label lblLanguage = null!;
        private SegmentedControl segLanguage = null!;
        private Label lblTheme = null!;
        private Label lblThemeName = null!;
        private ThemeSwatchRow themeSwatches = null!;
        private Label lblCustomColors = null!;
        private Label lblCustomHint = null!;
        private ColorWell wellBackground = null!;
        private ColorWell wellForeground = null!;
        private ColorWell wellAccent = null!;

        private Label lblHint = null!;
        private Label lblDataPath = null!;
        private FlatButton btnDefaults = null!;
        private FlatButton btnCancel = null!;
        private FlatButton btnOk = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            SuspendLayout();

            // ---- 自绘标题栏 ----
            pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 56),
                TabStop = false
            };

            lblTitle = new Label
            {
                AutoSize = true,
                Location = new Point(24, 18),
                Text = "设置"
            };

            btnClose = new FlatButton
            {
                Glyph = Design.IconGlyph.Close,
                GlyphSize = 15,
                Location = new Point(508, 14),
                Size = new Size(28, 28),
                TabStop = false,
                Variant = ButtonVariant.Icon
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnClose });

            // ---- 计时 ----
            cardTimer = new SurfacePanel
            {
                Location = new Point(20, 64),
                Size = new Size(252, 210)
            };

            lblSectionTimer = new Label { AutoSize = true, Location = new Point(16, 14), Text = "计时" };
            lblFocus = new Label { AutoSize = true, Location = new Point(16, 50), Text = "专注时长" };
            stpFocus = new StepperField { Location = new Point(102, 46), Size = new Size(134, 32) };
            lblShort = new Label { AutoSize = true, Location = new Point(16, 90), Text = "短休息" };
            stpShort = new StepperField { Location = new Point(102, 86), Size = new Size(134, 32) };
            lblLong = new Label { AutoSize = true, Location = new Point(16, 130), Text = "长休息" };
            stpLong = new StepperField { Location = new Point(102, 126), Size = new Size(134, 32) };
            lblRounds = new Label { AutoSize = true, Location = new Point(16, 170), Text = "长休间隔" };
            stpRounds = new StepperField { Location = new Point(102, 166), Size = new Size(134, 32) };

            cardTimer.Controls.AddRange(new Control[]
            {
                lblSectionTimer, lblFocus, stpFocus, lblShort, stpShort, lblLong, stpLong, lblRounds, stpRounds
            });

            // ---- 提醒 ----
            cardAlerts = new SurfacePanel
            {
                Location = new Point(20, 286),
                Size = new Size(252, 126)
            };

            lblSectionAlerts = new Label { AutoSize = true, Location = new Point(16, 14), Text = "提醒" };
            lblSound = new Label { AutoSize = true, Location = new Point(16, 46), Text = "结束提示音" };
            swSound = new ToggleSwitch { Location = new Point(194, 43) };
            lblNotify = new Label { AutoSize = true, Location = new Point(16, 86), Text = "系统通知" };
            swNotify = new ToggleSwitch { Location = new Point(194, 83) };

            cardAlerts.Controls.AddRange(new Control[] { lblSectionAlerts, lblSound, swSound, lblNotify, swNotify });

            // ---- 窗口 ----
            cardWindow = new SurfacePanel
            {
                Location = new Point(288, 64),
                Size = new Size(252, 168)
            };

            lblSectionWindow = new Label { AutoSize = true, Location = new Point(16, 14), Text = "窗口" };
            lblTopMost = new Label { AutoSize = true, Location = new Point(16, 46), Text = "窗口置顶" };
            swTopMost = new ToggleSwitch { Location = new Point(194, 43) };
            lblTray = new Label { AutoSize = true, Location = new Point(16, 86), Text = "关闭时最小化到托盘" };
            swTray = new ToggleSwitch { Location = new Point(194, 83) };
            lblExpand = new Label { AutoSize = true, Location = new Point(16, 126), Text = "启动时展开任务面板" };
            swExpand = new ToggleSwitch { Location = new Point(194, 123) };

            cardWindow.Controls.AddRange(new Control[]
            {
                lblSectionWindow, lblTopMost, swTopMost, lblTray, swTray, lblExpand, swExpand
            });

            // ---- 外观 ----
            cardAppearance = new SurfacePanel
            {
                Location = new Point(288, 244),
                Size = new Size(252, 278)
            };

            lblSectionAppearance = new Label { AutoSize = true, Location = new Point(16, 14), Text = "外观" };
            lblLanguage = new Label { AutoSize = true, Location = new Point(16, 44), Text = "语言" };
            segLanguage = new SegmentedControl { Location = new Point(16, 68), Size = new Size(220, 30) };
            lblTheme = new Label { AutoSize = true, Location = new Point(16, 108), Text = "主题" };
            lblThemeName = new Label
            {
                Location = new Point(120, 108),
                Size = new Size(116, 20),
                Text = string.Empty,
                TextAlign = ContentAlignment.MiddleRight
            };
            themeSwatches = new ThemeSwatchRow { Location = new Point(16, 130), Size = new Size(220, 34) };
            lblCustomColors = new Label { AutoSize = true, Location = new Point(16, 172), Text = "自定义颜色" };
            lblCustomHint = new Label
            {
                AutoSize = true,
                Location = new Point(16, 192),
                Text = "选择「自定义」主题后可调"
            };
            wellBackground = new ColorWell { Location = new Point(16, 212), Size = new Size(68, 50) };
            wellForeground = new ColorWell { Location = new Point(88, 212), Size = new Size(68, 50) };
            wellAccent = new ColorWell { Location = new Point(160, 212), Size = new Size(68, 50) };

            cardAppearance.Controls.AddRange(new Control[]
            {
                lblSectionAppearance, lblLanguage, segLanguage, lblTheme, lblThemeName, themeSwatches,
                lblCustomColors, lblCustomHint, wellBackground, wellForeground, wellAccent
            });

            // ---- 底部 ----
            lblHint = new Label
            {
                AutoSize = true,
                Location = new Point(20, 524),
                Text = "提示"
            };

            lblDataPath = new Label
            {
                AutoEllipsis = true,
                Location = new Point(20, 540),
                Size = new Size(520, 16),
                Text = string.Empty
            };

            btnDefaults = new FlatButton
            {
                Location = new Point(20, 558),
                Size = new Size(120, 36),
                Text = "恢复默认",
                Variant = ButtonVariant.Secondary
            };

            btnCancel = new FlatButton
            {
                Location = new Point(356, 558),
                Size = new Size(88, 36),
                Text = "取消",
                Variant = ButtonVariant.Ghost
            };

            btnOk = new FlatButton
            {
                Location = new Point(452, 558),
                Size = new Size(88, 36),
                Text = "确定",
                Variant = ButtonVariant.Primary
            };

            // ---- 窗体 ----
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(560, 610);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Settings";

            Controls.AddRange(new Control[]
            {
                pnlHeader, cardTimer, cardAlerts, cardWindow, cardAppearance, lblHint, lblDataPath, btnDefaults, btnCancel, btnOk
            });

            ResumeLayout(false);
        }
    }
}
