using System.Drawing;
using System.Drawing.Drawing2D;

namespace PotatoClock.Design;

/// <summary>需要在界面上出现的图标（按 SF Symbols 的线条风格用 GDI+ 手绘）。</summary>
public enum IconGlyph
{
    Play,
    Pause,
    Reset,
    Skip,
    Sliders,
    ChevronDown,
    Close,
    Plus,
    MinusCircle,

    Minus,
    Check
}

/// <summary>
/// 图标绘制。所有图标在 24×24 的设计空间里描述，绘制时缩放到目标矩形，
/// 线条使用圆头圆角，风格接近 SF Symbols。
/// </summary>
public static class Icons
{
    private const float Design = 24f;

    public static void Draw(Graphics graphics, IconGlyph glyph, RectangleF bounds, Color color, float strokeWidth = 1.9f)
    {
        if (bounds.Width <= 0f || bounds.Height <= 0f)
        {
            return;
        }

        var scale = Math.Min(bounds.Width, bounds.Height) / Design;
        var state = graphics.Save();
        try
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(
                bounds.X + ((bounds.Width - (Design * scale)) / 2f),
                bounds.Y + ((bounds.Height - (Design * scale)) / 2f));
            graphics.ScaleTransform(scale, scale);

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            using var brush = new SolidBrush(color);
            DrawGlyph(graphics, glyph, pen, brush, color);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    /// <summary>把填充图形的顶点轻微圆角化（描边同色再填充）。</summary>
    public static void FillRoundedShape(Graphics graphics, GraphicsPath path, Color color, float radius)
    {
        using var fatPen = new Pen(color, radius)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        graphics.DrawPath(fatPen, path);
        using var brush = new SolidBrush(color);
        graphics.FillPath(brush, path);
    }

    private static void DrawGlyph(Graphics graphics, IconGlyph glyph, Pen pen, Brush brush, Color color)
    {
        switch (glyph)
        {
            case IconGlyph.Play:
                {
                    using var path = new GraphicsPath();
                    path.AddPolygon(new[]
                    {
                        new PointF(8.6f, 5.4f),
                        new PointF(18.4f, 12f),
                        new PointF(8.6f, 18.6f)
                    });
                    FillRoundedShape(graphics, path, color, 2.6f);
                    break;
                }

            case IconGlyph.Pause:
                {
                    FillRoundedRect(graphics, brush, 7.6f, 5.2f, 3.4f, 13.6f, 1.7f);
                    FillRoundedRect(graphics, brush, 13f, 5.2f, 3.4f, 13.6f, 1.7f);
                    break;
                }

            case IconGlyph.Reset:
                {
                    // 逆时针开口圆 + 箭头
                    graphics.DrawArc(pen, 5.4f, 5.4f, 13.2f, 13.2f, -55f, 285f);
                    using var arrow = new GraphicsPath();
                    arrow.AddPolygon(new[]
                    {
                        new PointF(6.1f, 6.3f),
                        new PointF(11.6f, 5.1f),
                        new PointF(8.4f, 10.2f)
                    });
                    FillRoundedShape(graphics, arrow, color, 1.4f);
                    break;
                }

            case IconGlyph.Skip:
                {
                    using var path = new GraphicsPath();
                    path.AddPolygon(new[]
                    {
                        new PointF(6.4f, 5.6f),
                        new PointF(15.4f, 12f),
                        new PointF(6.4f, 18.4f)
                    });
                    FillRoundedShape(graphics, path, color, 2.4f);
                    FillRoundedRect(graphics, brush, 16.6f, 5.6f, 2.6f, 12.8f, 1.3f);
                    break;
                }

            case IconGlyph.Sliders:
                {
                    // Apple 的 slider.horizontal.3：三条横线 + 旋钮
                    graphics.DrawLine(pen, 5.4f, 7.4f, 18.6f, 7.4f);
                    graphics.DrawLine(pen, 5.4f, 12f, 18.6f, 12f);
                    graphics.DrawLine(pen, 5.4f, 16.6f, 18.6f, 16.6f);

                    using var knobBrush = new SolidBrush(color);
                    graphics.FillEllipse(knobBrush, 8.1f, 5.4f, 4f, 4f);
                    graphics.FillEllipse(knobBrush, 13.1f, 10f, 4f, 4f);
                    graphics.FillEllipse(knobBrush, 7.1f, 14.6f, 4f, 4f);
                    break;
                }

            case IconGlyph.ChevronDown:
                graphics.DrawLines(pen, new[]
                {
                    new PointF(8.5f, 10.4f),
                    new PointF(12f, 14.2f),
                    new PointF(15.5f, 10.4f)
                });
                break;

            case IconGlyph.Close:
                graphics.DrawLine(pen, 8.4f, 8.4f, 15.6f, 15.6f);
                graphics.DrawLine(pen, 15.6f, 8.4f, 8.4f, 15.6f);
                break;

            case IconGlyph.Plus:
                graphics.DrawLine(pen, 12f, 7.4f, 12f, 16.6f);
                graphics.DrawLine(pen, 7.4f, 12f, 16.6f, 12f);
                break;

            case IconGlyph.Minus:
                graphics.DrawLine(pen, 7.6f, 12f, 16.4f, 12f);
                break;

            case IconGlyph.MinusCircle:
                graphics.DrawEllipse(pen, 5.2f, 5.2f, 13.6f, 13.6f);
                graphics.DrawLine(pen, 8.6f, 12f, 15.4f, 12f);
                break;

            case IconGlyph.Check:
                graphics.DrawLines(pen, new[]
                {
                    new PointF(6.8f, 12.6f),
                    new PointF(10.6f, 16.2f),
                    new PointF(17.2f, 8.4f)
                });
                break;
        }
    }

    private static void FillRoundedRect(Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2f;
        path.AddArc(x, y, diameter, diameter, 180, 90);
        path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
        path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
        path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
