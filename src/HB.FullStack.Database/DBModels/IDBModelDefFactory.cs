using System;
using System.Collections.Generic;

using HB.FullStack.Database.Convert;

namespace HB.FullStack.Database.DbModels
{
    public interface IDbModelDefFactory
    {
        DbModelDef? GetDef<T>() where T : class, IDbModel;

        DbModelDef? GetDef(Type? modelType);

        IEnumerable<DbModelDef> GetAllDefsByDbSchema(string dbSchema);

        IDbPropertyConverter? GetPropertyTypeConverter(Type modelType, string propertyName);


    }
}