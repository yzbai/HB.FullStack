using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.Entity
{
    public interface IDatabaseEntityMapper
    {
        IList<T> To<T>(IDataReader reader) where T : DatabaseEntity, new();

        void To<T>(IDataReader reader, T item) where T : DatabaseEntity, new();

        IList<Tuple<TSource, TTarget>> To<TSource, TTarget>(IDataReader reader)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        IList<Tuple<TSource, TTarget2, TTarget3>> To<TSource, TTarget2, TTarget3>(IDataReader reader)
            where TSource : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
            where TTarget3 : DatabaseEntity, new();
    }
}