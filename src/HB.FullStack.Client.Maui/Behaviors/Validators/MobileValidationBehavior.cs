
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Behaviors;

using HB.FullStack.Common.Validate;

namespace HB.FullStack.Client.Maui.Behaviors
{
    public class MobileValidationBehavior : ValidationBehavior
    {

        protected override ValueTask<bool> ValidateAsync(object? value, CancellationToken token)
        {
            return new ValueTask<bool>(ValidationMethods.IsMobilePhone(value?.ToString()));
        }
    }
}
