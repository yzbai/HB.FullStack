using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Mobile.Skia
{
    public struct SKRatioPoint : IEquatable<SKRatioPoint>
    {
        public static SKRatioPoint Empty => new SKRatioPoint(0, 0);

        public float XRatio { get; set; }

        public float YRatio { get; set; }

        public bool IsEmpty => XRatio == 0 && YRatio == 0;

        public SKRatioPoint(float xRatio, float yRatio)
        {
            XRatio = xRatio;
            YRatio = yRatio;
        }

        public override bool Equals(object obj)
        {
            if (obj is SKRatioPoint point)
            {
                return Equals(point);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(XRatio, YRatio);
        }

        public static bool operator ==(SKRatioPoint left, SKRatioPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SKRatioPoint left, SKRatioPoint right)
        {
            return !(left == right);
        }

        public bool Equals(SKRatioPoint other)
        {
            return XRatio == other.XRatio && YRatio == other.YRatio;
        }
    }
}
