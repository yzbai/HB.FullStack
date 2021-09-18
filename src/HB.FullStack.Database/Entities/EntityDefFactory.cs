using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Database.Entities
{
    internal static class EntityDefFactory
    {
        public static int VarcharDefaultLength { get; set; }

        private static readonly IDictionary<Type, EntityDef> _defDict = new Dictionary<Type, EntityDef>();

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="databaseEngine"></param>
        /// <exception cref="DatabaseException"></exception>
        public static void Initialize(IDatabaseEngine databaseEngine)
        {
            DatabaseCommonSettings databaseSettings = databaseEngine.DatabaseSettings;

            VarcharDefaultLength = databaseSettings.DefaultVarcharLength == 0 ? LengthConvention.DEFAULT_VARCHAR_LENGTH : databaseSettings.DefaultVarcharLength;

            IEnumerable<Type> allEntityTypes;

            if (databaseSettings.AssembliesIncludeEntity.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(entityTypeCondition);
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(databaseSettings.AssembliesIncludeEntity, entityTypeCondition);
            }

            IDictionary<string, EntitySetting> entitySchemaDict = ConstructeSchemaDict(databaseSettings, databaseEngine, allEntityTypes);

            WarmUp(allEntityTypes, databaseEngine.EngineType, entitySchemaDict);

            static bool entityTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(DatabaseEntity)) && !t.IsAbstract;
            }
        }

        /// <summary>
        /// WarmUp
        /// </summary>
        /// <param name="allEntityTypes"></param>
        /// <param name="engineType"></param>
        /// <param name="entitySchemaDict"></param>
        /// <exception cref="DatabaseException"></exception>
        private static void WarmUp(IEnumerable<Type> allEntityTypes, EngineType engineType, IDictionary<string, EntitySetting> entitySchemaDict)
        {
            foreach (var t in allEntityTypes)
            {
                _defDict[t] = CreateEntityDef(t, engineType, entitySchemaDict);
            }
        }

        private static IDictionary<string, EntitySetting> ConstructeSchemaDict(DatabaseCommonSettings databaseSettings, IDatabaseEngine databaseEngine, IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, EntitySetting> fileConfiguredDict = databaseSettings.EntitySettings.ToDictionary(t => t.EntityTypeFullName);

            IDictionary<string, EntitySetting> resusltEntitySchemaDict = new Dictionary<string, EntitySetting>();

            foreach (Type type in allEntityTypes)
            {
                DatabaseAttribute? attribute = type.GetCustomAttribute<DatabaseAttribute>();

                fileConfiguredDict.TryGetValue(type.FullName!, out EntitySetting? fileConfigured);

                EntitySetting entitySchema = new EntitySetting
                {
                    EntityTypeFullName = type.FullName!
                };

                if (attribute != null)
                {
                    entitySchema.DatabaseName = attribute.DatabaseName.IsNullOrEmpty() ? databaseEngine.FirstDefaultDatabaseName : attribute.DatabaseName!;

                    if (attribute.TableName.IsNullOrEmpty())
                    {
                        entitySchema.TableName = "tb_";

                        if (type.Name.EndsWith(attribute.SuffixToRemove, GlobalSettings.Comparison))
                        {
                            entitySchema.TableName += type.Name.Substring(0, type.Name.Length - attribute.SuffixToRemove.Length).ToLower(GlobalSettings.Culture);
                        }
                        else
                        {
                            entitySchema.TableName += type.Name.ToLower(GlobalSettings.Culture);
                        }
                    }
                    else
                    {
                        entitySchema.TableName = attribute.TableName!;
                    }

                    entitySchema.Description = attribute.Description;
                    entitySchema.ReadOnly = attribute.ReadOnly;
                }

                //文件配置可以覆盖代码中的配置
                if (fileConfigured != null)
                {
                    if (!string.IsNullOrEmpty(fileConfigured.DatabaseName))
                    {
                        entitySchema.DatabaseName = fileConfigured.DatabaseName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.TableName))
                    {
                        entitySchema.TableName = fileConfigured.TableName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.Description))
                    {
                        entitySchema.Description = fileConfigured.Description;
                    }

                    entitySchema.ReadOnly = fileConfigured.ReadOnly;
                }

                //做最后的检查，有可能两者都没有定义
                if (entitySchema.DatabaseName.IsNullOrEmpty())
                {
                    entitySchema.DatabaseName = databaseEngine.FirstDefaultDatabaseName;
                }

                if (entitySchema.TableName.IsNullOrEmpty())
                {
                    entitySchema.TableName = "tb_" + type.Name.ToLower(GlobalSettings.Culture);
                }

                resusltEntitySchemaDict.Add(type.FullName!, entitySchema);
            }

            return resusltEntitySchemaDict;
        }

        public static EntityDef? GetDef<T>() where T : DatabaseEntity
        {
            return GetDef(typeof(T));
        }

        public static EntityDef? GetDef(Type? entityType)
        {
            if(entityType == null)
            {
                return null;
            }

            if (_defDict.TryGetValue(entityType, out EntityDef? entityDef))
            {
                return entityDef;
            }

            return null;
        }

        /// <summary>
        /// CreateEntityDef
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="engineType"></param>
        /// <param name="entitySchemaDict"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static EntityDef CreateEntityDef(Type entityType, EngineType engineType, IDictionary<string, EntitySetting> entitySchemaDict)
        {
            //GlobalSettings.Logger.LogInformation($"{entityType} : {entityType.GetHashCode()}");

            if (!entitySchemaDict!.TryGetValue(entityType.FullName!, out EntitySetting? dbSchema))
            {
                throw Exceptions.EntityError(type:entityType.FullName,"", cause: "不是Entity，或者没有DatabaseEntityAttribute.");
            }

            EntityDef entityDef = new EntityDef
            {
                IsIdAutoIncrement = entityType.IsSubclassOf(typeof(AutoIncrementIdEntity)),
                IsIdGuid = entityType.IsSubclassOf(typeof(GuidEntity)),
                EntityType = entityType,
                EntityFullName = entityType.FullName!,
                DatabaseName = dbSchema.DatabaseName,
                TableName = dbSchema.TableName
            };
            entityDef.DbTableReservedName = SqlHelper.GetReserved(entityDef.TableName!, engineType);
            entityDef.DatabaseWriteable = !dbSchema.ReadOnly;

            //确保Id排在第一位，在EntityMapper中，判断reader.GetValue(0)为DBNull,则为Null
            var orderedProperties = entityType.GetProperties().OrderBy(p => p, new PropertyOrderComparer());

            foreach (PropertyInfo info in orderedProperties)
            {
                EntityPropertyAttribute? entityPropertyAttribute = info.GetCustomAttribute<EntityPropertyAttribute>(true);

                if (entityPropertyAttribute == null)
                {
                    IgnoreEntityPropertyAttribute? ignoreAttribute = info.GetCustomAttribute<IgnoreEntityPropertyAttribute>(true);

                    if(ignoreAttribute != null)
                    {
                        continue;
                    }
                    else
                    {
                        entityPropertyAttribute = new EntityPropertyAttribute();
                    }

                    if (info.Name == nameof(Entity.LastUser))
                    {
                        entityPropertyAttribute.MaxLength = LengthConvention.LAST_USER_MAX_LENGTH;
                    }
                }

                EntityPropertyDef propertyDef = CreatePropertyDef(entityDef, info, entityPropertyAttribute, engineType);

                entityDef.FieldCount++;

                if (propertyDef.IsUnique)
                {
                    entityDef.UniqueFieldCount++;
                }

                entityDef.PropertyDefs.Add(propertyDef);
                entityDef.PropertyDict.Add(propertyDef.Name, propertyDef);
            }

            return entityDef;
        }

        /// <summary>
        /// CreatePropertyDef
        /// </summary>
        /// <param name="entityDef"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="propertyAttribute"></param>
        /// <param name="engineType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static EntityPropertyDef CreatePropertyDef(EntityDef entityDef, PropertyInfo propertyInfo, EntityPropertyAttribute propertyAttribute, EngineType engineType)
        {
            EntityPropertyDef propertyDef = new EntityPropertyDef
            {
                EntityDef = entityDef,
                Name = propertyInfo.Name,
                Type = propertyInfo.PropertyType
            };
            propertyDef.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyDef.Type);

            propertyDef.SetMethod = ReflectUtil.GetPropertySetterMethod(propertyInfo, entityDef.EntityType)
                ?? throw Exceptions.EntityError(type: entityDef.EntityFullName, propertyName: propertyInfo.Name, cause:"实体属性缺少Set方法. ");

            propertyDef.GetMethod = ReflectUtil.GetPropertyGetterMethod(propertyInfo, entityDef.EntityType)
                ?? throw Exceptions.EntityError(type: entityDef.EntityFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Get方法. ");


            propertyDef.IsIndexNeeded = propertyAttribute.NeedIndex;
            propertyDef.IsNullable = !propertyAttribute.NotNull;
            propertyDef.IsUnique = propertyAttribute.Unique;
            propertyDef.DbMaxLength = propertyAttribute.MaxLength > 0 ? (int?)propertyAttribute.MaxLength : null;
            propertyDef.IsLengthFixed = propertyAttribute.FixedLength;

            propertyDef.DbReservedName = SqlHelper.GetReserved(propertyDef.Name, engineType);
            propertyDef.DbParameterizedName = SqlHelper.GetParameterized(propertyDef.Name);

            if (propertyAttribute.Converter != null)
            {
                propertyDef.TypeConverter = (ITypeConverter)Activator.CreateInstance(propertyAttribute.Converter)!;
            }

            //判断是否是主键
            PrimaryKeyAttribute? primaryAttribute = propertyInfo.GetCustomAttribute<PrimaryKeyAttribute>(false);

            if (primaryAttribute != null)
            {
                entityDef.PrimaryKeyPropertyDef = propertyDef;
                propertyDef.IsPrimaryKey = true;
                propertyDef.IsAutoIncrementPrimaryKey = primaryAttribute is AutoIncrementPrimaryKeyAttribute;
                propertyDef.IsNullable = false;
                propertyDef.IsForeignKey = false;
                propertyDef.IsUnique = true;
            }
            else
            {
                //判断是否外键
                ForeignKeyAttribute? atts2 = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(false);

                if (atts2 != null)
                {
                    propertyDef.IsAutoIncrementPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    propertyDef.IsNullable = true;
                    propertyDef.IsUnique = false;
                }
            }

            return propertyDef;
        }

        public static IEnumerable<EntityDef> GetAllDefsByDatabase(string databaseName)
        {
            return _defDict.Values.Where(def => databaseName.Equals(def.DatabaseName, GlobalSettings.ComparisonIgnoreCase));
        }

        public static ITypeConverter? GetPropertyTypeConverter(Type entityType, string propertyName)
        {
            return GetDef(entityType)?.GetPropertyDef(propertyName)!.TypeConverter;
        }
    }
}