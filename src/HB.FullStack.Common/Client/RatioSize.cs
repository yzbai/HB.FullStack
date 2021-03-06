using System;



namespace HB.FullStack.Common
{

    public struct RatioSize : IEquatable<RatioSize>
    {
        public float WidthRatio { get; set; }

        public float HeightRatio { get; set; }

        public RatioSize(float widthRatio, float heightRatio)
        {
            WidthRatio = widthRatio;
            HeightRatio = heightRatio;
        }



        public override bool Equals(object obj)
        {
            if (obj is RatioSize ratioSize)
            {
                return Equals(ratioSize);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WidthRatio, HeightRatio);
        }

        public static bool operator ==(RatioSize left, RatioSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RatioSize left, RatioSize right)
        {
            return !(left == right);
        }

        public bool Equals(RatioSize other)
        {
            return other.WidthRatio == WidthRatio && other.HeightRatio == HeightRatio;
        }
    }
}
