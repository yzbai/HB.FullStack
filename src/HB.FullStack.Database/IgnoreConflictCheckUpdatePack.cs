using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using HB.FullStack.Common.PropertyTrackable;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public class IgnoreConflictCheckUpdatePack
    {
        [Required]
        public object? Id { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();
    }

    public static class IgnoreConflictCheckUpdatePackExtensions
    {
        public static IgnoreConflictCheckUpdatePack ThrowIfNotValid(this IgnoreConflictCheckUpdatePack updatePack)
        {
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

        public static IList<IgnoreConflictCheckUpdatePack> ThrowIfNotValid(this IList<IgnoreConflictCheckUpdatePack> updatePacks)
        {
            foreach (var updatePack in updatePacks)
            {
                updatePack.ThrowIfNotValid();
            }

            return updatePacks;
        }

        public static IgnoreConflictCheckUpdatePack ToIgnoreConflictCheckUpdatePack(this PropertyChangePack changePack, DbModelDef modelDef)
        {
            if (!changePack.AddtionalProperties.TryGetValue(modelDef.PrimaryKeyPropertyDef.Name, out JsonElement idElement))
            {
                throw DbExceptions.ChangedPropertyPackError("ChangePack的AddtionalProperties中缺少Id", changePack, modelDef.FullName);
            }

            IgnoreConflictCheckUpdatePack dbPack = new IgnoreConflictCheckUpdatePack
            {
                Id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, idElement)
            };

            foreach (PropertyChange cp in changePack.PropertyChanges.Values)
            {
                //TODO: 性能改进，使用Emit，可以省去查询这一步, Input: changePack.PropertyNames(需要提前order), modelDef

                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(cp.PropertyName)
                    ?? throw DbExceptions.ChangedPropertyPackError($"ChangePack包含未知的property:{cp.PropertyName}", changePack, modelDef.FullName);

                dbPack.PropertyNames.Add(cp.PropertyName);
                dbPack.NewPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue));
            }

            return dbPack;
        }
    }
}
