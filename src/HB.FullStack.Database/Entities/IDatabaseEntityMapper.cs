#nullable enable

using HB.FullStack.Common.Entities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace HB.FullStack.Database.Entities
{
    internal interface IDatabaseEntityMapper
    {

        IList<T> ToList<T>(DatabaseEntityDef entityDef, IDataReader reader) where T : Entity, new();

        IList<Tuple<TSource, TTarget?>> ToList<TSource, TTarget>(DatabaseEntityDef sourceEntityDef, DatabaseEntityDef targetEntityDef, IDataReader reader)
            where TSource : Entity, new()
            where TTarget : Entity, new();


        IList<Tuple<TSource, TTarget2?, TTarget3?>> ToList<TSource, TTarget2, TTarget3>(DatabaseEntityDef sourceEntityDef, DatabaseEntityDef targetEntityDef1, DatabaseEntityDef targetEntityDef2, IDataReader reader)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new();
    }
}