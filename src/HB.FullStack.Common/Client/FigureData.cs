using System;

namespace HB.FullStack.Common
{
    /// <summary>
    /// Figure中的数据与状态
    /// </summary>
    public abstract class FigureData : IEquatable<FigureData>
    {
        public FigureState State { get; set; }

        public bool Equals(FigureData? other)
        {
            if(other is null)
            {
                return false;
            }

            return State == other.State && EqualsCore(other);
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

        public override bool Equals(object obj)
        {
            if (obj is FigureData data)
            {
                return Equals(data);
            }

            return false;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = GetHashCodeCore();
            hashCode.Add(State);

            return hashCode.ToHashCode();
        }

        protected abstract bool EqualsCore(FigureData other);

        protected abstract HashCode GetHashCodeCore();

    }
}
