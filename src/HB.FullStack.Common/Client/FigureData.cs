using System;

namespace HB.FullStack.Common
{
    public abstract class FigureData : IEquatable<FigureData>
    {
        public FigureState State { get; set; }

        public bool Equals(FigureData? other)
        {
            if(ReferenceEquals(other, null))
            {
                return false;
            }

            return State == other.State && EqualsImpl(other);
        }

        public static bool operator ==(FigureData? d1, FigureData? d2)
        {
            if (ReferenceEquals(d1, d2))
            {
                return true;
            }

            if (ReferenceEquals(d1, null) || ReferenceEquals(d2, null))
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
            HashCode hashCode = GetHashCodeImpl();
            hashCode.Add(State);

            return hashCode.ToHashCode();
        }

        protected abstract bool EqualsImpl(FigureData other);

        protected abstract HashCode GetHashCodeImpl();

    }
}
