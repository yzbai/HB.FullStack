using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Entity
{
    public class SignInToken : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [ForeignKey(typeof(User))]
        [GuidEntityProperty]
        public string UserGuid { get; set; }

        [Required]
        [EntityProperty("RefreshToken")]
        public string RefreshToken { get; set; }

        [EntityProperty("ExpireAt")]
        public DateTimeOffset? ExpireAt { get; set; }

        [EntityProperty("RefreshCount")]
        public long RefreshCount { get; set; } = 0;

        [EntityProperty("Blacked")]
        public bool Blacked { get; set; } = false;


        #region Client

        [Required]
        [EntityProperty("ClientId")]
        public string ClientId { get; set; }

        [Required]
        [EntityProperty("ClientType")]
        public string ClientType { get; set; }

        [EntityProperty("ClientVersion")]
        public string ClientVersion { get; set; }

        [EntityProperty("Address")]
        public string ClientAddress { get; set; }

        [EntityProperty("Client IP")]
        public string ClientIp { get; set; }

        #endregion
    }
}