using System;
using System.Collections.Generic;

using HB.FullStack.Database.Converter;

namespace HB.FullStack.Database.Entities
{
    public interface IEntityDefFactory
    {
        EntityDef? GetDef<T>() where T : DatabaseEntity;

        EntityDef? GetDef(Type? entityType);

        IEnumerable<EntityDef> GetAllDefsByDatabase(string databaseName);

        ITypeConverter? GetPropertyTypeConverter(Type entityType, string propertyName);
    }
}