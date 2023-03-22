using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Identity.Context;

namespace HB.FullStack.Identity
{
    public abstract class SignInContext : ValidatableObject, IHasAudience
    {
        /// <summary>
        /// Sign to where
        /// </summary>
        [Required]
        public string Audience { get; set; } = default!;

        public bool RememberMe { get; set; }

        public SignInExclusivity Exclusivity { get; set; } = SignInExclusivity.None;

        [ValidatedObject(CanBeNull = false)]
        public ClientInfos ClientInfos { get; set; }

        [ValidatedObject(CanBeNull = false)]
        public DeviceInfos DeviceInfos { get; set; }

        public SignInContext(
            string audience, 
            bool rememberMe, 
            SignInExclusivity exclusivity, 
            ClientInfos clientInfos,
            DeviceInfos deviceInfos)
        {
            Audience = audience;
            RememberMe = rememberMe;
            Exclusivity = exclusivity;
            ClientInfos = clientInfos;
            DeviceInfos = deviceInfos;
        }
    }
}