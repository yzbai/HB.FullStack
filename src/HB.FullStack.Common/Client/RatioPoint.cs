using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 比例点
    /// </summary>
    public struct RatioPoint : IEquatable<RatioPoint>
    {
        public static RatioPoint Empty => new RatioPoint(0, 0);

        /// <summary>
        /// X = XRation * Canvas.Width
        /// </summary>
        public float XRatio { get; set; }

        /// <summary>
        /// Y = YRatio * Canvas.Height
        /// </summary>
        public float YRatio { get; set; }

        public bool IsEmpty() => XRatio == 0 && YRatio == 0;

        public RatioPoint(float xRatio, float yRatio)
        {
            XRatio = xRatio;
            YRatio = yRatio;
        }

        public override bool Equals(object obj)
        {
            if (obj is RatioPoint point)
            {
                return Equals(point);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(XRatio, YRatio);
        }

        public static bool operator ==(RatioPoint left, RatioPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RatioPoint left, RatioPoint right)
        {
            return !(left == right);
        }

        public bool Equals(RatioPoint other)
        {
            return XRatio == other.XRatio && YRatio == other.YRatio;
        }
    }
}
