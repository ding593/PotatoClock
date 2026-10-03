using System.Drawing;
using System.Drawing.Drawing2D;

namespace PotatoClock.Controls;

/// <summary>应用 logo（嵌入的 logo.png），按高质量插值缩放绘制。</summary>
public sealed class LogoMark : ThemedControl
{
    private static Image? _image;
    private static bool _loaded;

    public LogoMark()
    {
        TabStop = false;
        Size = new Size(24, 24);
    }

    /// <summary>共享的 logo 位图（只加载一次）。</summary>
    public static Image? SharedImage
    {
        get
        {
            if (_loaded)
            {
                return _image;
            }

            _loaded = true;
            try
            {
                using var stream = typeof(LogoMark).Assembly.GetManifestResourceStream("PotatoClock.logo.png");
                if (stream is not null)
                {
                    _image = Image.FromStream(stream);
                }
            }
            catch (Exception)
            {
                _image = null;
            }

            return _image;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackdropColor);

        var image = SharedImage;
        if (image is null)
        {
            return;
        }

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(image, new RectangleF(0f, 0f, Width, Height));
    }
}
