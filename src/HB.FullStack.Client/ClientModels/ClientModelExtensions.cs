using System.Linq;

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
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => pv.Value)
            };

            return changedPack;
        }
    }
}
