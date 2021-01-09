
using HB.FullStack.Common.Validate;

using Xamarin.CommunityToolkit.Behaviors;
using Xamarin.CommunityToolkit.Behaviors.Internals;

namespace HB.FullStack.Mobile.Behaviors
{
	public class MobileValidationBehavior :  ValidationBehavior
	{
		protected override bool Validate(object? value)
		{
			return ValidationMethods.IsMobilePhone(value?.ToString());
		}
	}
}
