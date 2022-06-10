
using System;

namespace HB.FullStack.Common.Figures
{
    /// <summary>
    /// 用于绘制Figure的参数,只与UI有关。比如钟表，只与钟表样貌有关，与几点几分无关。
    /// </summary>
    public abstract class FigureDrawInfo : IEquatable<FigureDrawInfo>
    {
        public bool Equals(FigureDrawInfo? other)
        {
            if (other is null)
            {
                return false;
            }

            return EqualsCore(other);
        }

        public static bool operator ==(FigureDrawInfo? d1, FigureDrawInfo? d2)
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

        public static bool operator !=(FigureDrawInfo? d1, FigureDrawInfo? d2)
        {
            return !(d1 == d2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is FigureDrawInfo data)
            {
                return Equals(data);
            }

            return false;
        }

        public abstract override int GetHashCode();

        protected abstract bool EqualsCore(FigureDrawInfo other);

    }
}
