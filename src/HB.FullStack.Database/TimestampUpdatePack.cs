using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{

    /// <summary>
    /// 使用Timestamp来解决冲突的更新包
    /// </summary>
    public class TimestampUpdatePack
    {
        [Required]
        public object? Id { get; set; }

        [Required]
        public long? OldTimestamp { get; set; }

        /// <summary>
        /// 如果不指定，则使用TimeUtil.Timestamp
        /// </summary>
        public long? NewTimestamp { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();

    }

    public static class TimestampUpdatePackExtensions
    {
        public static TimestampUpdatePack ThrowIfNotValid(this TimestampUpdatePack updatePack)
        {
            if (updatePack.OldTimestamp == null || updatePack.OldTimestamp <= 638000651894004864)
            {
                throw DbExceptions.ConflictCheckError($"");
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count)
            {
                throw DbExceptions.UpdatePackCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdatePackEmpty();
            }

            return updatePack;
        }

        public static IList<TimestampUpdatePack> ThrowIfNotValid(this IList<TimestampUpdatePack> updatePacks)
        {
            foreach (var updatePack in updatePacks)
            {
                updatePack.ThrowIfNotValid();
            }

            return updatePacks;
        }

        public static TimestampUpdatePack ToTimestampUpdatePack(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} is not ITimestamp, but convert PropertyChangePack to TimestampUpdatePack.");
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement primaryKeyElement))
            {
                throw DbExceptions.ChangedPropertyPackError("PropertyChangePack的AddtionalProperties中缺少PrimaryKey", changePack, modelDef.FullName);
            }

            //if (!changePack.AddtionalProperties.TryGetValue(nameof(ITimestamp.Timestamp), out JsonElement timestampElement))
            //{
            //    throw DbExceptions.ChangedPropertyPackError("PropertyChangePack的AddtionalProperties中缺少Timestamp", changePack, modelDef.FullName);
            //}

            TimestampUpdatePack dbPack = new TimestampUpdatePack
            {
                Id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, primaryKeyElement)
            };

            string timestampPropertyName = modelDef.TimestampPropertyDef!.Name;

            foreach (PropertyChange cp in changePack.PropertyChanges)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(cp.PropertyName)
                    ?? throw DbExceptions.ChangedPropertyPackError($"ChangePack包含未知的property:{cp.PropertyName}", changePack, modelDef.FullName);

                if (propertyDef.Name == timestampPropertyName)
                {
                    dbPack.OldTimestamp = cp.OldValue.To<long>();
                    dbPack.NewTimestamp = cp.NewValue.To<long>();

                    continue;
                }

                dbPack.PropertyNames.Add(cp.PropertyName);
                dbPack.NewPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue));
            }

            if (dbPack.OldTimestamp == null || dbPack.NewTimestamp == null)
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} convert PropertyChangePack to TimestampUpdatepack, but no Timestamp old new values.");
            }

            return dbPack;
        }
    }
}
