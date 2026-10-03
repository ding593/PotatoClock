using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PotatoClock.Design;

/// <summary>
/// 无边框窗口的外观辅助：圆角裁剪、发丝边框、系统阴影、拖动。
/// </summary>
public static class WindowChrome
{
    private const int GclStyle = -26;
    private const int CsDropShadow = 0x00020000;
    private const int WmNclButtonDown = 0x00A1;
    private const int HtCaption = 0x0002;

    /// <summary>按窗口尺寸设置圆角裁剪区域。</summary>
    public static void ApplyRoundedRegion(Form form, int radius = Radii.Window)
    {
        if (form.Width <= 0 || form.Height <= 0)
        {
            return;
        }

        using var path = Shapes.RoundedPath(new RectangleF(0f, 0f, form.Width, form.Height), radius);
        var previous = form.Region;
        form.Region = new Region(path);
        previous?.Dispose();
    }

    /// <summary>沿圆角画 1 物理像素的发丝边框。</summary>
    public static void PaintBorder(Graphics graphics, Form form, Color color, int radius = Radii.Window)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Shapes.RoundedPath(new RectangleF(0.5f, 0.5f, form.Width - 1f, form.Height - 1f), radius);
        using var pen = new Pen(color, 1f);
        graphics.DrawPath(pen, path);
    }

    /// <summary>给无边框窗口加系统级柔和阴影（不可用时静默忽略）。</summary>
    public static void EnableDropShadow(Form form)
    {
        try
        {
            var style = GetClassLongPtr(form.Handle, GclStyle);
            SetClassLongPtr(form.Handle, GclStyle, new IntPtr(style.ToInt64() | CsDropShadow));
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or NotSupportedException)
        {
            // 没有阴影同样可用
        }
    }

    /// <summary>开始拖动窗口（用于自绘标题栏）。</summary>
    public static void BeginDrag(Form form)
    {
        ReleaseCapture();
        SendMessage(form.Handle, WmNclButtonDown, HtCaption, 0);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
    private static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW", SetLastError = true)]
    private static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
