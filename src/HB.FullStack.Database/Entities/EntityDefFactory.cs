using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database.Entities
{
    internal class EntityDefFactory2 : IEntityDefFactory
    {
        /// <summary>
        /// 这里不用ConcurrentDictionary。是因为在初始化时，就已经ConstructEntityDefs，后续只有read，没有write
        /// </summary>
        private readonly IDictionary<Type, EntityDef> _defDict = new Dictionary<Type, EntityDef>();

        public EntityDefFactory2(IDatabaseEngine databaseEngine)
        {
            DatabaseCommonSettings databaseSettings = databaseEngine.DatabaseSettings;

            IEnumerable<Type> allEntityTypes;

            if (databaseSettings.Assemblies.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(entityTypeCondition);
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(databaseSettings.Assemblies, entityTypeCondition);
            }

            IDictionary<string, EntitySetting> entitySettingDict = ConstructeSettingDict(databaseSettings, databaseEngine, allEntityTypes);

            ConstructEntityDefs(allEntityTypes, databaseEngine.EngineType, entitySettingDict);

            static bool entityTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(DatabaseEntity)) && !t.IsAbstract;
            }
        }

        private void ConstructEntityDefs(IEnumerable<Type> allEntityTypes, EngineType engineType, IDictionary<string, EntitySetting> entitySettingDict)
        {
            foreach (var t in allEntityTypes)
            {
                _defDict[t] = CreateEntityDef(t, engineType, entitySettingDict);
            }
        }

        private static IDictionary<string, EntitySetting> ConstructeSettingDict(DatabaseCommonSettings databaseSettings, IDatabaseEngine databaseEngine, IEnumerable<Type> allEntityTypes)
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
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
                            entitySchema.TableName += type.Name[..^attribute.SuffixToRemove.Length].ToLower(GlobalSettings.Culture);
#elif NETSTANDARD2_0
                            entitySchema.TableName += type.Name.Substring(0, type.Name.Length - attribute.SuffixToRemove.Length).ToLower(GlobalSettings.Culture);
#endif
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

        public EntityDef? GetDef<T>() where T : DatabaseEntity
        {
            return GetDef(typeof(T));
        }

        public EntityDef? GetDef(Type? entityType)
        {
            if (entityType == null)
            {
                return null;
            }

            if (_defDict.TryGetValue(entityType, out EntityDef? entityDef))
            {
                return entityDef;
            }

            return null;
        }

        private static EntityDef CreateEntityDef(Type entityType, EngineType engineType, IDictionary<string, EntitySetting> entitySettingDict)
        {
            //GlobalSettings.Logger.LogInformation($"{entityType} : {entityType.GetHashCode()}");

            if (!entitySettingDict!.TryGetValue(entityType.FullName!, out EntitySetting? dbSchema))
            {
                throw DatabaseExceptions.EntityError(type: entityType.FullName, "", cause: "不是Entity，或者没有DatabaseEntityAttribute.");
            }

            EntityDef entityDef = new EntityDef
            {
                IsIdAutoIncrement = entityType.IsSubclassOf(typeof(AutoIncrementIdEntity)),
                IsIdGuid = entityType.IsSubclassOf(typeof(GuidEntity)),
                IsIdLong = entityType.IsSubclassOf(typeof(LongIdEntity)),
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

                    if (ignoreAttribute != null)
                    {
                        continue;
                    }
                    else
                    {
                        entityPropertyAttribute = new EntityPropertyAttribute();
                    }

                    if (info.Name == nameof(Entity.LastUser))
                    {
                        entityPropertyAttribute.MaxLength = DefaultLengthConventions.MAX_LAST_USER_LENGTH;
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
                ?? throw DatabaseExceptions.EntityError(type: entityDef.EntityFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Set方法. ");

            propertyDef.GetMethod = ReflectUtil.GetPropertyGetterMethod(propertyInfo, entityDef.EntityType)
                ?? throw DatabaseExceptions.EntityError(type: entityDef.EntityFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Get方法. ");

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
                    propertyDef.IsUnique = atts2.IsUnique;
                }
            }

            return propertyDef;
        }

        public IEnumerable<EntityDef> GetAllDefsByDatabase(string databaseName)
        {
            return _defDict.Values.Where(def => databaseName.Equals(def.DatabaseName, GlobalSettings.ComparisonIgnoreCase));
        }

        public ITypeConverter? GetPropertyTypeConverter(Type entityType, string propertyName)
        {
            return GetDef(entityType)?.GetPropertyDef(propertyName)!.TypeConverter;
        }
    }
}