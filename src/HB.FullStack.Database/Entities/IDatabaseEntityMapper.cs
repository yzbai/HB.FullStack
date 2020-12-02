#nullable enable

using HB.FullStack.Common.Entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace HB.FullStack.Database.Entities
{
    internal interface IDatabaseEntityMapper
    {
        
        IList<T> ToList<T>(IDataReader reader) where T : Entity, new();

        
        void ToObject<T>(IDataReader reader, T item) where T : Entity, new();

        
        IList<Tuple<TSource, TTarget?>> ToList<TSource, TTarget>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget : Entity, new();

        
        IList<Tuple<TSource, TTarget2?, TTarget3?>> ToList<TSource, TTarget2, TTarget3>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new();
    }
}