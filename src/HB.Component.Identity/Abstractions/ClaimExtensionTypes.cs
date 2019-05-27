﻿

using System.Security.Claims;

namespace System.Security.Claims
{
    public static class ClaimExtensionTypes
    {
        public const string Name = ClaimsIdentity.DefaultNameClaimType;

        public const string Role = ClaimsIdentity.DefaultRoleClaimType;

        public const string IsEmailConfirmed = "HB.Identity.IsEmailConfirmed";

        public const string IsMobileConfirmed = "HB.Identity.IsMobileConfirmed";

        public const string Authtoken = "HB.Identity.Authtoken";
        public const string SignInTokenGuid = "HB.Identity.SignInTokenGuid";

        public const string IconUrl = "HB.Identity.IconUrl";

        public const string UserName = "HB.Identity.UserName";

        public const string SecurityStamp = "HB.Identity.SecurityStamp";

        public const string MobilePhone = "HB.Identity.MobilePhone";
        public const string Email = "HB.Identity.Email";
        public const string Audience = "aud";
        public const string UserGuid = "HB.Identity.Guid";

        public const string ClientId = "HB.Identity.ClientId";
    }
}