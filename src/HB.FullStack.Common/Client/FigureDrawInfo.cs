using System;

namespace HB.FullStack.Common
{
    public abstract class FigureDrawInfo : IEquatable<FigureDrawInfo>
    {
        public bool Equals(FigureDrawInfo? other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return EqualsImpl(other);
        }

        public static bool operator ==(FigureDrawInfo? d1, FigureDrawInfo? d2)
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

        public static bool operator !=(FigureDrawInfo? d1, FigureDrawInfo? d2)
        {
            return !(d1 == d2);
        }

        public override bool Equals(object obj)
        {
            if (obj is FigureDrawInfo data)
            {
                return Equals(data);
            }

            return false;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = GetHashCodeImpl();

            return hashCode.ToHashCode();
        }

        protected abstract bool EqualsImpl(FigureDrawInfo other);

        protected abstract HashCode GetHashCodeImpl();

    }
}
