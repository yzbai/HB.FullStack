using System;

using SkiaSharp;

namespace HB.FullStack.XamarinForms.Skia
{
    public struct SKRatioSize : IEquatable<SKRatioSize>
    {
        public float WidthRatio { get; set; }

        public float HeightRatio { get; set; }

        public SKRatioSize(float widthRatio, float heightRatio)
        {
            WidthRatio = widthRatio;
            HeightRatio = heightRatio;
        }

        public SKSize ToSKSize(float containerWidth, float containerHeight)
        {
            return new SKSize(WidthRatio * containerWidth, HeightRatio * containerHeight);
        }

        public SKSize ToSKSize(SKSize containerSize)
        {
            return new SKSize(WidthRatio * containerSize.Width, HeightRatio * containerSize.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj is SKRatioSize ratioSize)
            {
                return Equals(ratioSize);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WidthRatio, HeightRatio);
        }

        public static bool operator ==(SKRatioSize left, SKRatioSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SKRatioSize left, SKRatioSize right)
        {
            return !(left == right);
        }

        public bool Equals(SKRatioSize other)
        {
            return other.WidthRatio == WidthRatio && other.HeightRatio == HeightRatio;
        }
    }
}
