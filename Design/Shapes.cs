using System.Drawing;
using System.Drawing.Drawing2D;

namespace PotatoClock.Design;

/// <summary>通用几何辅助。</summary>
public static class Shapes
{
    /// <summary>圆角矩形路径（半径自动收窄到不超过短边的一半）。</summary>
    public static GraphicsPath RoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0f || rect.Height <= 0f)
        {
            return path;
        }

        var diameter = Math.Max(0f, Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height)));
        if (diameter <= 0.5f)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
