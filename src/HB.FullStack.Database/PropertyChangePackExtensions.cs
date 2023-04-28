using System;
using System.Text.Json;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public static class PropertyChangePackExtensions
    {
        public static UpdatePackTimeless ToUpdatePackTimeless(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (changePack == null || changePack.PropertyChanges.IsNullOrEmpty())
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack为空或者Id为null", changePack, modelDef.ModelFullName);
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.ModelFullName);
            }

            UpdatePackTimeless dbPack = new UpdatePackTimeless
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

        public static UpdatePackTimestamp ToUpdatePackTimestamp(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (changePack == null || changePack.PropertyChanges.IsNullOrEmpty())
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack为空或者Id为null", changePack, modelDef.ModelFullName);
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.ModelFullName);
            }

            UpdatePackTimestamp dbPack = new UpdatePackTimestamp
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
