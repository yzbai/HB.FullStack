using System;

namespace HB.FullStack.XamarinForms.Skia
{
    public abstract class SKFigureDrawData : IEquatable<SKFigureDrawData>
    {
        public bool IsSelected { get; set; }

        public bool Equals(SKFigureDrawData? other)
        {
            if(ReferenceEquals(other, null))
            {
                return false;
            }

            return EqualsImpl(other);
        }

        public static bool operator ==(SKFigureDrawData? d1, SKFigureDrawData? d2)
        {
            if (ReferenceEquals(d1, d2))
            {
                return true;
            }

            if (ReferenceEquals(d1, null) || ReferenceEquals(d2, null))
            {
                return false;
            }

            return d1.EqualsImpl(d2);
        }

        public static bool operator !=(SKFigureDrawData? d1, SKFigureDrawData? d2)
        {
            return !(d1 == d2);
        }

        public override bool Equals(object obj)
        {
            if (obj is SKFigureDrawData data)
            {
                return Equals(data);
            }

            return false;
        }

        protected abstract bool EqualsImpl(SKFigureDrawData other);

        public abstract override int GetHashCode();
    }
}
