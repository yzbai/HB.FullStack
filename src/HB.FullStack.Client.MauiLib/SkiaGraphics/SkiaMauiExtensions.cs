using Microsoft.Maui.Graphics;

using SkiaSharp;

namespace Microsoft.Maui.Graphics
{
    public static class SkiaMauiExtensions
    {
        public static SKPoint ToSK(this PointF point)
        {
            return new SKPoint(point.X, point.Y);
        }
    }
}
