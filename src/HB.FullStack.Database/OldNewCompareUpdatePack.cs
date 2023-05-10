using System;
using System.Collections.Generic;
using System.Text.Json;
using HB.FullStack.Common.PropertyTrackable;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 使用新旧值比较来解决冲突
    /// </summary>
    public class OldNewCompareUpdatePack
    {
        public object? Id { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();

        public IList<object?> OldPropertyValues { get; set; } = new List<object?>();
    }

    public static class OldNewCompareUpdatePackExtensions
    {
        public static OldNewCompareUpdatePack ThrowIfNotValid(this OldNewCompareUpdatePack updatePack)
        {
            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count || updatePack.OldPropertyValues.Count != updatePack.PropertyNames.Count)
            {
                throw DbExceptions.UpdateUsingTimestampListCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdateUsingTimestampListEmpty();
            }

            return updatePack;
        }

        public static OldNewCompareUpdatePack ToOldNewCompareUpdatePack(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (changePack == null || changePack.PropertyChanges.IsNullOrEmpty())
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack为空或者Id为null", changePack, modelDef.ModelFullName);
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.ModelFullName);
            }

            OldNewCompareUpdatePack dbPack = new OldNewCompareUpdatePack
            {
                Id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, idElement)
            };

            foreach (PropertyChange cp in changePack.PropertyChanges)
            {
                //TODO: 性能改进，使用Emit，可以省去查询这一步, Input: changePack.PropertyNames(需要提前order), modelDef

                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(cp.PropertyName)
                    ?? throw DbExceptions.ChangedPropertyPackError($"ChangePack包含未知的property:{cp.PropertyName}", changePack, modelDef.ModelFullName);

                dbPack.PropertyNames.Add(cp.PropertyName);
                dbPack.NewPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue));
                dbPack.OldPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.OldValue));
            }

            return dbPack;
        }
    }
}
