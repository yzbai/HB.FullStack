using Microsoft.Maui.Graphics;

using SkiaSharp;

namespace HB.FullStack.Client.Maui.Controls.SkiaGraphics
{
    public static class SkiaMauiExtensions
    {
        public static SKPoint ToSK(this PointF point)
        {
            return new SKPoint(point.X, point.Y);
        }
    }
}
