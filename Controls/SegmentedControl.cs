using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotatoClock.App;
using PotatoClock.Design;

namespace PotatoClock.Controls;

/// <summary>Apple 风格分段控件：胶囊轨道 + 滑动选中药丸。</summary>
public sealed class SegmentedControl : ThemedControl
{
    private readonly object _pillToken = new();

    private string[] _items = Array.Empty<string>();
    private int _selectedIndex;
    private double _pill;
    private double _hoverPill = -1d;

    public SegmentedControl()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = false;
        Height = 32;
        Cursor = Cursors.Hand;
        Font = Typography.Get(10f, semibold: true);
    }

    public event EventHandler? SelectionChanged;

    public string[] Items
    {
        get => _items;
        set
        {
            _items = value ?? Array.Empty<string>();
            _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, _items.Length - 1));
            _pill = _selectedIndex;
            Invalidate();
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var clamped = Math.Clamp(value, 0, Math.Max(0, _items.Length - 1));
            if (clamped == _selectedIndex && _pill == clamped)
            {
                return;
            }

            _selectedIndex = clamped;
            Animator.Animate(_pillToken, _pill, clamped, Motion.Toggle, position =>
            {
                _pill = position;
                Invalidate();
            }, EaseKind.Standard);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>不触发事件地设置（初始化用）。</summary>
    public void SetSelectedSilently(int index)
    {
        _selectedIndex = Math.Clamp(index, 0, Math.Max(0, _items.Length - 1));
        _pill = _selectedIndex;
        Invalidate();
    }

    protected override void OnThemeChanged() => SyncBackdrop();

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = HitTest(e.X);
        if (index != _hoverPill)
        {
            _hoverPill = index;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverPill = -1d;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var index = HitTest(e.X);
        if (index >= 0 && index < _items.Length)
        {
            SelectedIndex = index;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        if (_items.Length == 0)
        {
            return;
        }

        var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);

        // 轨道比所在表面「下沉」一档，选中药丸比轨道抬起一档（与 Apple 分段控件一致）
        var track = Theme.IsDark
            ? ThemeCatalog.Over(Theme.WindowBackground, BackdropColor, 0.88)
            : ThemeCatalog.Over(Color.FromArgb(255, 60, 60, 67), BackdropColor, 0.06);
        PaintRounded(graphics, rect, Radii.Control, track, Theme.Separator);

        var itemWidth = (rect.Width - 6f) / _items.Length;
        var pillRect = new RectangleF(rect.X + 3f + ((float)_pill * itemWidth), rect.Y + 3f, itemWidth, rect.Height - 6f);
        var pill = Theme.IsDark ? Theme.SurfaceHover : Theme.Surface;
        PaintRounded(graphics, pillRect, Math.Max(3f, Radii.Control - 3f), pill, null);

        for (var i = 0; i < _items.Length; i++)
        {
            var itemRect = new Rectangle(
                (int)Math.Round(rect.X + 3f + (i * itemWidth)),
                (int)rect.Y + 3,
                (int)Math.Round(itemWidth),
                (int)rect.Height - 6);
            var isSelected = i == _selectedIndex;
            var isHover = Math.Abs(_hoverPill - i) < 0.5d;
            var color = isSelected ? Theme.Foreground : isHover ? Theme.Secondary : Theme.Secondary;

            TextRenderer.DrawText(
                graphics,
                _items[i],
                Font,
                itemRect,
                color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
    }

    private int HitTest(int x)
    {
        if (_items.Length == 0)
        {
            return -1;
        }

        var itemWidth = (Width - 6f) / _items.Length;
        var index = (int)Math.Floor((x - 3f) / itemWidth);
        return index < 0 || index >= _items.Length ? -1 : index;
    }
}
