using System;
using System.Collections.Generic;

using HB.FullStack.Database.Converter;

namespace HB.FullStack.Database.DatabaseModels
{
    public interface IDatabaseModelDefFactory
    {
        DatabaseModelDef? GetDef<T>() where T : DatabaseModel;

        DatabaseModelDef? GetDef(Type? modelType);

        IEnumerable<DatabaseModelDef> GetAllDefsByDatabase(string databaseName);

        ITypeConverter? GetPropertyTypeConverter(Type modelType, string propertyName);
    }
}