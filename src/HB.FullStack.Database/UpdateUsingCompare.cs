using System;
using System.Collections.Generic;
using System.Text.Json;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public class UpdateUsingCompare
    {
        public object? Id { get; set; }

        public long? NewTimestamp { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();

        public IList<object?> OldPropertyValues { get; set; } = new List<object?>();
    }

    public static class PropertyChangePackExtensions
    {
        public static UpdateUsingCompare ToUpdateUsingCompare(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (changePack == null || changePack.PropertyChanges.IsNullOrEmpty())
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack为空或者Id为null", changePack, modelDef.ModelFullName);
            }

            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.ModelFullName);
            }

            UpdateUsingCompare dbPack = new UpdateUsingCompare
            {
                Id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, idElement)
            };

            foreach (PropertyChange cp in changePack.PropertyChanges)
            {
                if (cp.PropertyName == nameof(ITimestampModel.Timestamp))
                {
                    dbPack.NewTimestamp = cp.NewValue.To<long>();
                    continue;
                }

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
