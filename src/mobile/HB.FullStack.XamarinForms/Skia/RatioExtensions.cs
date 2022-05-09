
using HB.FullStack.Common;
using HB.FullStack.Common.Figures;

using SkiaSharp;

namespace HB.FullStack.Common
{
    public static class RatioExtensions
    {
        public static SKPoint ToSKPoint(this RatioPoint sKRatioPoint, float containerWidth, float containerHeight)
        {
            return new SKPoint(sKRatioPoint.XRatio * containerWidth, sKRatioPoint.YRatio * containerHeight);
        }

        public static SKPoint ToSKPoint(this RatioPoint ratioPoint, SKSize containerSize)
        {
            return new SKPoint(ratioPoint.XRatio * containerSize.Width, ratioPoint.YRatio * containerSize.Height);
        }

        public static SKSize ToSKSize(this RatioSize ratioSize, float containerWidth, float containerHeight)
        {
            return new SKSize(ratioSize.WidthRatio * containerWidth, ratioSize.HeightRatio * containerHeight);
        }

        public static SKSize ToSKSize(this RatioSize ratioSize, SKSize containerSize)
        {
            return new SKSize(ratioSize.WidthRatio * containerSize.Width, ratioSize.HeightRatio * containerSize.Height);
        }
    }
}
