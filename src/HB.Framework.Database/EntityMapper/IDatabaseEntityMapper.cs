using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.Entity
{
    internal interface IDatabaseEntityMapper
    {
        IList<T> ToList<T>(IDataReader reader) where T : DatabaseEntity, new();

        void ToObject<T>(IDataReader reader, T item) where T : DatabaseEntity, new();

        IList<Tuple<TSource, TTarget>> ToList<TSource, TTarget>(IDataReader reader)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        IList<Tuple<TSource, TTarget2, TTarget3>> ToList<TSource, TTarget2, TTarget3>(IDataReader reader)
            where TSource : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
            where TTarget3 : DatabaseEntity, new();
    }
}