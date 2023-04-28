
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Behaviors;

using HB.FullStack.Common.Validate;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Behaviors
{
    public class SmsCodeValidationBehavior : ValidationBehavior
    {
        public static readonly BindableProperty SmsCodeLengthProperty = BindableProperty.Create(nameof(SmsCodeLength), typeof(int), typeof(SmsCodeValidationBehavior), defaultValue: 6);

        public int SmsCodeLength { get => (int)GetValue(SmsCodeLengthProperty); set => SetValue(SmsCodeLengthProperty, value); }

        protected override ValueTask<bool> ValidateAsync(object? value, CancellationToken token)
        {
            return new ValueTask<bool>(ValidationMethods.IsSmsCode(value?.ToString(), SmsCodeLength));
        }
    }
}
