namespace System.Security.Claims
{
    public static class ClaimExtensionTypes
    {
        public const string Role = ClaimsIdentity.DefaultRoleClaimType;

        public const string SignInTokenGuid = "HB.SignInTokenGuid";

        public const string SecurityStamp = "HB.SecurityStamp";

        public const string Audience = "aud";

        public const string UserGuid = "HB.UserGuid";

        public const string DeviceId = "HB.DeviceId";

        public const string ApiKeyName = "HB.ApiKeyName";

        //public const string Name = ClaimsIdentity.DefaultNameClaimType;

        //public const string IsEmailConfirmed = "HB.Identity.IsEmailConfirmed";

        //public const string IsMobileConfirmed = "HB.Identity.IsMobileConfirmed";

        //public const string Authtoken = "HB.Identity.Authtoken";

        //public const string IconUrl = "HB.Identity.IconUrl";

        public const string LoginName = "HB.Identity.LoginName";

        //public const string MobilePhone = "HB.Identity.MobilePhone";
        //public const string Email = "HB.Identity.Email";
    }
}