#nullable enable

using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.Entities
{
    internal interface IDatabaseEntityMapper
    {
        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        IList<T> ToList<T>(IDataReader reader) where T : Entity, new();

        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        void ToObject<T>(IDataReader reader, T item) where T : Entity, new();

        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        IList<Tuple<TSource, TTarget?>> ToList<TSource, TTarget>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget : Entity, new();

        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        IList<Tuple<TSource, TTarget2?, TTarget3?>> ToList<TSource, TTarget2, TTarget3>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new();
    }
}