
using HB.FullStack.Common.Validate;

using Xamarin.CommunityToolkit.Behaviors;
using Xamarin.CommunityToolkit.Behaviors.Internals;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Behaviors
{
	public class MobileValidationBehavior :  ValidationBehavior
	{
		protected override bool Validate(object? value)
		{
			return ValidationMethods.IsMobilePhone(value?.ToString());
		}
	}

	public class SmsCodeValidationBehavior : ValidationBehavior
	{
		public static readonly BindableProperty SmsCodeLengthProperty = BindableProperty.Create(nameof(SmsCodeLength), typeof(int), typeof(SmsCodeValidationBehavior), defaultValue:6);

		public int SmsCodeLength { get => (int)GetValue(SmsCodeLengthProperty); set => SetValue(SmsCodeLengthProperty, value); }

		protected override bool Validate(object? value)
		{
			return ValidationMethods.IsSmsCode(value?.ToString(), SmsCodeLength);
		}
	}
}
