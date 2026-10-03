using PotatoClock.Controls;
using PotatoClock.Design;

namespace PotatoClock.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null!;

        private Panel pnlHeader = null!;
        private LogoMark logo = null!;
        private Label lblPhase = null!;
        private FlatButton btnSettings = null!;
        private FlatButton btnTasks = null!;
        private FlatButton btnClose = null!;
        private TimeDisplay timeDisplay = null!;
        private PhaseDots dots = null!;
        private Label lblCaption = null!;
        private FlatProgressBar progress = null!;
        private FlatButton btnToggle = null!;
        private FlatButton btnReset = null!;
        private FlatButton btnSkip = null!;
        private SurfacePanel card = null!;
        private Label lblTasks = null!;
        private Label lblTasksCount = null!;
        private Label lblEmpty = null!;
        private FlowLayoutPanel taskFlow = null!;
        private RoundedField taskField = null!;
        private FlatButton btnAdd = null!;

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

            // ---- 顶部：logo + 阶段 + 图标按钮（整条可拖动）----
            pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(340, 52),
                TabStop = false
            };

            logo = new LogoMark
            {
                Location = new Point(20, 15),
                Size = new Size(22, 22)
            };

            lblPhase = new Label
            {
                AutoSize = true,
                Location = new Point(50, 17),
                Text = "专注"
            };

            btnSettings = new FlatButton
            {
                Glyph = IconGlyph.Sliders,
                GlyphSize = 17,
                Location = new Point(228, 12),
                Size = new Size(28, 28),
                TabStop = false,
                Variant = ButtonVariant.Icon
            };

            btnTasks = new FlatButton
            {
                Glyph = IconGlyph.ChevronDown,
                GlyphSize = 17,
                Location = new Point(260, 12),
                Size = new Size(28, 28),
                TabStop = false,
                Variant = ButtonVariant.Icon
            };

            btnClose = new FlatButton
            {
                Glyph = IconGlyph.Close,
                GlyphSize = 15,
                Location = new Point(292, 12),
                Size = new Size(28, 28),
                TabStop = false,
                Variant = ButtonVariant.Icon
            };

            pnlHeader.Controls.AddRange(new Control[] { logo, lblPhase, btnSettings, btnTasks, btnClose });

            // ---- 时间、进度点、说明、进度条 ----
            timeDisplay = new TimeDisplay
            {
                Location = new Point(20, 56),
                Size = new Size(300, 76)
            };

            dots = new PhaseDots
            {
                Location = new Point(20, 138),
                Size = new Size(300, 10)
            };

            lblCaption = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(300, 16),
                Text = "第 1/4 个",
                TextAlign = ContentAlignment.MiddleCenter
            };

            progress = new FlatProgressBar
            {
                Location = new Point(20, 174),
                Size = new Size(300, 6)
            };

            // ---- 操作按钮 ----
            btnToggle = new FlatButton
            {
                Glyph = IconGlyph.Play,
                GlyphSize = 16,
                Location = new Point(20, 192),
                Size = new Size(140, 36),
                Text = "开始",
                Variant = ButtonVariant.Primary
            };

            btnReset = new FlatButton
            {
                Glyph = IconGlyph.Reset,
                GlyphSize = 15,
                Location = new Point(164, 192),
                Size = new Size(76, 36),
                Text = "重置",
                Variant = ButtonVariant.Secondary
            };

            btnSkip = new FlatButton
            {
                Glyph = IconGlyph.Skip,
                GlyphSize = 15,
                Location = new Point(244, 192),
                Size = new Size(76, 36),
                Text = "跳过",
                Variant = ButtonVariant.Secondary
            };

            // ---- 任务卡片 ----
            card = new SurfacePanel
            {
                Location = new Point(16, 252),
                Size = new Size(308, 254),
                Visible = false
            };

            lblTasks = new Label
            {
                AutoSize = true,
                Location = new Point(16, 14),
                Text = "任务"
            };

            lblTasksCount = new Label
            {
                Location = new Point(204, 17),
                Size = new Size(88, 16),
                Text = "0",
                TextAlign = ContentAlignment.MiddleRight
            };

            lblEmpty = new Label
            {
                Location = new Point(12, 84),
                Size = new Size(284, 20),
                Text = "还没有任务",
                TextAlign = ContentAlignment.MiddleCenter
            };

            taskFlow = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(12, 44),
                Size = new Size(284, 152),
                TabStop = false,
                WrapContents = false
            };

            taskField = new RoundedField
            {
                Location = new Point(12, 204),
                Size = new Size(232, 32)
            };

            btnAdd = new FlatButton
            {
                Glyph = IconGlyph.Plus,
                GlyphSize = 18,
                Location = new Point(250, 204),
                Size = new Size(38, 32),
                TabStop = false,
                Variant = ButtonVariant.Secondary
            };

            card.Controls.AddRange(new Control[] { lblTasks, lblTasksCount, lblEmpty, taskFlow, taskField, btnAdd });

            // ---- 窗体 ----
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(340, 246);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PotatoClock";

            Controls.AddRange(new Control[] { pnlHeader, timeDisplay, dots, lblCaption, progress, btnToggle, btnReset, btnSkip, card });

            ResumeLayout(false);
        }
    }
}
