using System;
using System.Collections.Generic;

using HB.FullStack.Database.Converter;

namespace HB.FullStack.Database.DatabaseModels
{
    public interface IDBModelDefFactory
    {
        DBModelDef? GetDef<T>() where T : DBModel;

        DBModelDef? GetDef(Type? modelType);

        IEnumerable<DBModelDef> GetAllDefsByDatabase(string databaseName);

        ITypeConverter? GetPropertyTypeConverter(Type modelType, string propertyName);
    }
}