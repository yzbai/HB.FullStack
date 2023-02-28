using System;
using System.Linq;

using HB.FullStack.Common.Meta;

namespace HB.FullStack.Common.PropertyTrackable
{
    public static class IPropertyTrackableObjectExtensions
    {
        public static PropertyChangePack GetChangePack(this IPropertyTrackableObject model, bool mergeMultipleChanges = true)
        {
            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(model);

            PropertyChangePack changePack = new PropertyChangePack
            {
                PropertyChanges = model.GetPropertyChanges(mergeMultipleChanges),
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => SerializeUtil.ToJsonElement(pv.Value))
            };

            return changePack;
        }
    }
}