using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Meta;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Client.ClientModels
{
    public static class ClientModelExtensions
    {
        public static ChangedPack GetChangedPack(this ClientDbModel model)
        {
            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(model);

            ChangedPack changedPack = new ChangedPack
            {
                Id = model.Id,
                ChangedProperties = model.GetChangedProperties(),
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => SerializeUtil.ToJsonElement(pv.Value))
            };

            return changedPack;
        }
    }
}
