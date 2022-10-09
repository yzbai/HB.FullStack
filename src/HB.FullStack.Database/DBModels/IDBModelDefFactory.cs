using System;
using System.Collections.Generic;

using HB.FullStack.Database.Convert;

namespace HB.FullStack.Database.DbModels
{
    public interface IDbModelDefFactory
    {
        DbModelDef? GetDef<T>() where T : DbModel;

        DbModelDef? GetDef(Type? modelType);

        IEnumerable<DbModelDef> GetAllDefsByDatabase(string databaseName);

        IDbPropertyConverter? GetPropertyTypeConverter(Type modelType, string propertyName);
    }
}