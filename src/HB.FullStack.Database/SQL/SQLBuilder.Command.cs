#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    internal partial class SQLBuilder
    {
        //TODO: 优化
        private static IDataParameter[] GetCommandParameters<T>(IDatabaseEngine databaseEngine, DatabaseEntityDef entityDef, T entity, int number = 0) where T : Entity, new()
        {
            IDataParameter[] parameters = new IDataParameter[entityDef.FieldCount - 1];
            for (int i = 0; i < entityDef.FieldCount; ++i)
            {
                DatabaseEntityPropertyDef propertyDef = entityDef.PropertyDefs[i];

                parameters[i] = databaseEngine.CreateParameter(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    DatabaseTypeConverter.TypeValueToDbValue(propertyDef.GetValueFrom(entity), propertyDef));
            }

            return parameters;
        }

        private static string CreateAddCommandText(DatabaseEntityDef entityDef, DatabaseEngineType engineType, bool returnId, int number = 0)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DatabaseEntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                args.Append($"{propertyDef.DbReservedName},");

                values.Append($"{propertyDef.DbParameterizedName}_{number},");
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId ? $"select {GetLastInsertIdStatement(engineType)};" : string.Empty;

            return $"insert into {entityDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        private static string CreateUpdateCommandText(DatabaseEntityDef entityDef, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (DatabaseEntityPropertyDef propertyInfo in entityDef.PropertyDefs)
            {
                if (propertyInfo.IsAutoIncrementPrimaryKey || propertyInfo.Name == nameof(Entity.Guid) || propertyInfo.Name == nameof(Entity.Deleted))
                {
                    continue;
                }

                args.Append($" {propertyInfo.DbReservedName}={propertyInfo.DbParameterizedName}_{number},");
            }

            args.RemoveLast();

            DatabaseEntityPropertyDef idProperty = entityDef.GetPropertyDef(nameof(Entity.Id))!;
            DatabaseEntityPropertyDef deletedProperty = entityDef.GetPropertyDef(nameof(Entity.Deleted))!;
            DatabaseEntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            StringBuilder where = new StringBuilder();

            where.Append($"{idProperty.DbReservedName}={idProperty.DbParameterizedName}_{number} AND ");
            where.Append($"{versionProperty.DbReservedName}={versionProperty.DbParameterizedName}_{number} - 1 AND ");
            where.Append($"{deletedProperty.DbReservedName}=0");

            return $"UPDATE {entityDef.DbTableReservedName} SET {args} WHERE {where};";
        }

        private static string CreateDeleteCommandText(DatabaseEntityDef entityDef, int number = 0)
        {
            return CreateUpdateCommandText(entityDef, number);
        }

        private static string CreateSelectCommandText(params DatabaseEntityDef[] entityDefs)
        {
            StringBuilder builder = new StringBuilder("SELECT ");

            foreach (DatabaseEntityDef entityDef in entityDefs)
            {
                string DbTableReservedName = entityDef.DbTableReservedName;

                foreach (DatabaseEntityPropertyDef propertyDef in entityDef.PropertyDefs)
                {
                    builder.Append($"{DbTableReservedName}.{propertyDef.DbReservedName},");
                }
            }

            builder.RemoveLast();

            return builder.ToString();
        }
    }
}
