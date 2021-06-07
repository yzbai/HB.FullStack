using System;
using System.Globalization;

using HB.FullStack.Mobile.Utils;

using Xamarin.Essentials;

namespace HB.FullStack.Mobile
{
    public static class AppStates
    {
        //#region Introduced

        //private const string _introduced_preference_key = nameof(_introduced_preference_key);

        //private static bool? _introduced;

        //public static bool Introduced
        //{
        //    get
        //    {
        //        if (_introduced.HasValue)
        //        {
        //            return _introduced.Value;
        //        }

        //        _introduced = GetPreferenceIntroduced();

        //        if (_introduced.Value && VersionTracking.IsFirstLaunchForCurrentBuild)
        //        {
        //            _introduced = false;
        //            SetPreferenceIntroduced(false);
        //        }

        //        return _introduced.Value;
        //    }
        //    set
        //    {
        //        _introduced = value;
        //        SetPreferenceIntroduced(value);
        //    }
        //}

        //private static bool GetPreferenceIntroduced()
        //{
        //    string? stored = PreferenceHelper.PreferenceGetAsync(_introduced_preference_key).Result;

        //    if (stored == null)
        //    {
        //        return false;
        //    }

        //    return Convert.ToBoolean(stored, CultureInfo.InvariantCulture);
        //}

        //private static void SetPreferenceIntroduced(bool introduced)
        //{
        //    PreferenceHelper.PreferenceSetAsync(_introduced_preference_key, introduced.ToString()).Fire();
        //}

        //#endregion

        //public static bool IsLogined
        //{
        //    get => UserPreferences.IsLogined;
        //    set
        //    {
        //        UserPreferences.Logout();
        //    }
        //}

        //public static bool Registered { get; private set; }

        //public static void OfflineDataUsed()
        //{

        //}
    }
}
