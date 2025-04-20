
using System;

namespace HB.FullStack.Common.Figures
{
    /// <summary>
    /// Figure中的数据与状态。也会影响到绘制。比如钟表的时间，是关键数据，而与钟表长什么样子无关。
    /// </summary>
    public abstract class FigureData : IEquatable<FigureData>
    {
        public FigureVisualState VisualState { get; set; }

        public bool Equals(FigureData? other)
        {
            if (other is null)
            {
                return false;
            }

            return VisualState == other.VisualState && EqualsCore(other);
        }

        public static bool operator ==(FigureData? d1, FigureData? d2)
        {
            if (ReferenceEquals(d1, d2))
            {
                return true;
            }

            if (d1 is null || d2 is null)
            {
                return false;
            }

            return d1.Equals(d2);
        }

        public static bool operator !=(FigureData? d1, FigureData? d2)
        {
            return !(d1 == d2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is FigureData data)
            {
                return Equals(data);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetHashCodeCore(), VisualState);
        }

        protected abstract bool EqualsCore(FigureData other);

        protected abstract int GetHashCodeCore();

    }
}
