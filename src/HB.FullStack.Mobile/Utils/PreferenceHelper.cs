using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Utils
{
    public static class PreferenceHelper
    {
        public static async Task<string?> PreferenceGetAsync(string key)
        {
            try
            {
                string? value = await SecureStorage.GetAsync(key).ConfigureAwait(false);


                return value;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Application.Current.Log(LogLevel.Critical, ex, $"SecureStorage Get 失败，很严重. key:{key}. Message:{ex.Message}");
                return null;
            }
        }

        public static async Task PreferenceSetAsync(string key, string? value)
        {
            try
            {
                await SecureStorage.SetAsync(key, value ?? string.Empty).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //TODO: Possible that device doesn't support secure storage on device.
                Application.Current.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");
                return;
            }
        }
    }
}
