using HB.Framework.Database.Entity;
using HB.Framework.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.AuthorizationServer
{
    [Serializable]
    public class SignInToken : DatabaseEntity
    {

        [DatabaseEntityProperty("")]
        public string SignInTokenIdentifier { get; set; }

        [DatabaseForeignKey("", typeof(User))]
        public long UserId { get; set; }

        [Required]
        [DatabaseEntityProperty("RefreshToken")]
        public string RefreshToken { get; set; }

        [DatabaseEntityProperty("ExpireAt")]
        public DateTimeOffset? ExpireAt { get; set; }

        [DatabaseEntityProperty("RefreshCount")]
        public long RefreshCount { get; set; } = 0;

        [DatabaseEntityProperty("Blacked")]
        public bool Blacked { get; set; } = false;


        #region Client

        [Required]
        [DatabaseEntityProperty("ClientId")]
        public string ClientId { get; set; }

        [Required]
        [DatabaseEntityProperty("ClientType")]
        public string ClientType { get; set; }

        [DatabaseEntityProperty("ClientVersion")]
        public string ClientVersion { get; set; }

        [DatabaseEntityProperty("Address")]
        public string ClientAddress { get; set; }

        [DatabaseEntityProperty("Client IP")]
        public string ClientIp { get; set; }

        #endregion
    }
}