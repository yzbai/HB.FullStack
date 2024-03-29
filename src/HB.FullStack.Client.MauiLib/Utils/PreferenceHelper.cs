﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace System
{
    //TODO: 如果SecurityStorage不支持，改用普通的Preference

    internal static class PreferenceHelper
    {
        private static bool? _securityNotSupported;

        public static bool SecurityStorageSupported
        {
            get
            {
                if (!_securityNotSupported.HasValue)
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

        static async Task<string?> GetAsync(string key)
        {
            try
            {
                if (SecurityStorageSupported)
                {
                    return await SecureStorage.GetAsync(key);
                }
                else
                {
                    return Preferences.Get(key, null);
                }
            }
            //catch (FeatureNotSupportedException ex)
            catch (Exception ex)
            {
                Globals.Logger?.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");

                SecurityStorageSupported = false;

                return await GetAsync(key);
            }
        }

        public static string? Get(string key)
        {
            return JoinableTasks.JoinableTaskFactory.Run(async () => await GetAsync(key));
        }

        static async Task SetAsync(string key, string value)
        {
            try
            {
                if (SecurityStorageSupported)
                {
                    if (value.IsNullOrEmpty())
                    {
                        SecureStorage.Remove(key);
                    }
                    else
                    {
                        await SecureStorage.SetAsync(key, value);
                    }
                }
                else
                {
                    Preferences.Set(key, value);
                }
            }
            //catch (FeatureNotSupportedException ex)
            catch (Exception ex)
            {
                Globals.Logger?.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");

                SecurityStorageSupported = false;

                await SetAsync(key, value);
            }
        }

        //TODO: 考虑非异步方法
        public static void Set(string key, string value)
        {
            JoinableTasks.JoinableTaskFactory.Run(async () => await SetAsync(key, value));
        }
    }
}
