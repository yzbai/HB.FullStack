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
    //TODO: 如果SecurityStorage不支持，改用普通的Preference
    public static class PreferenceHelper
    {
        private static bool? _securityNotSupported;

        public static bool SecurityStorageSupported
        {
            get 
            {
                if(!_securityNotSupported.HasValue)
                {
                    _securityNotSupported = Preferences.Get(nameof(SecurityStorageSupported), true);
                }

                return _securityNotSupported.Value;
            
            }
            set 
            {
                _securityNotSupported = value;
                Preferences.Set(nameof(SecurityStorageSupported), value);
            }
        }

        public static async Task<string?> PreferenceGetAsync(string key)
        {
            try
            {
                if (SecurityStorageSupported)
                {
                    return await SecureStorage.GetAsync(key).ConfigureAwait(false);
                }
                else
                {
                    return Preferences.Get(key, null);
                }
            }
            catch(FeatureNotSupportedException ex)
            {
                GlobalSettings.Logger.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");

                SecurityStorageSupported = false;

                return await PreferenceGetAsync(key).ConfigureAwait(false);
            }
        }

        public static async Task PreferenceSetAsync(string key, string value)
        {
            try
            {
                if (SecurityStorageSupported)
                {
                    await SecureStorage.SetAsync(key, value).ConfigureAwait(false);
                }
                else
                {
                    Preferences.Set(key, value);
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                GlobalSettings.Logger.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");

                SecurityStorageSupported = false;

                await PreferenceSetAsync(key, value).ConfigureAwait(false);
            }
        }
    }
}
