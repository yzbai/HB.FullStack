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
            if (!updatePack.OldTimestamp.HasValue || updatePack.OldTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.OldTimestamp ?? 0);
            }

            if (updatePack.NewTimestamp.HasValue && updatePack.NewTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.NewTimestamp.Value);
            }

            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count)
            {
                throw DbExceptions.UpdateUsingTimestampListCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdateUsingTimestampListEmpty();
            }

            return updatePack;
        }

        public static TimestampUpdatePack ToTimestampUpdatePack(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (changePack == null || changePack.PropertyChanges.IsNullOrEmpty())
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack为空或者Id为null", changePack, modelDef.ModelFullName);
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.ModelFullName);
            }

            TimestampUpdatePack dbPack = new TimestampUpdatePack
            {
                Id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, idElement)
            };

            foreach (PropertyChange cp in changePack.PropertyChanges)
            {
                if (cp.PropertyName == nameof(ITimestampModel.Timestamp))
                {
                    dbPack.OldTimestamp = cp.OldValue.To<long>();
                    dbPack.NewTimestamp = cp.NewValue.To<long>();
                    continue;
                }

                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(cp.PropertyName)
                    ?? throw DbExceptions.ChangedPropertyPackError($"ChangePack包含未知的property:{cp.PropertyName}", changePack, modelDef.ModelFullName);

                dbPack.PropertyNames.Add(cp.PropertyName);
                dbPack.NewPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue));
            }

            return dbPack;
        }
    }
}
