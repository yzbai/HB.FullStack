using System.Linq;

using HB.FullStack.Common.Meta;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Client.ClientModels
{
    public static class ClientModelExtensions
    {
        public static ChangedPack2 GetChangedPack(this ClientDbModel model)
        {
            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(model);

            ChangedPack2 changedPack = new ChangedPack2
            {
                Id = model.Id,
                ChangedProperties = model.GetChangedProperties(),
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => pv.Value)
            };

            return changedPack;
        }
    }
}
