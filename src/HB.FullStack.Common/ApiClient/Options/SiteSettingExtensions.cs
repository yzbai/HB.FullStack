namespace HB.FullStack.Common.ApiClient
{
    public static class SiteSettingExtensions
    {
        public static string GetHttpClientName(this SiteSetting siteSettings)
        {
            return $"{siteSettings.SiteName}_{siteSettings.Version ?? "0"}";
        }
    }
}
