using HB.Framework.Database.Entity;
using HB.Framework.Identity;
using System;

namespace HB.Framework.AuthorizationServer
{
    [Serializable]
    public class ThirdPartyLogin : DatabaseEntity
    {
        [DatabaseForeignKey("用户ID", typeof(User))]
        public long UserId { get; set; }

        [DatabaseEntityProperty("登陆提供者", Length=500)]
        public string LoginProvider { get; set; }

        [DatabaseEntityProperty("登陆key", Length=500)]
        public string ProviderKey { get; set; }

        [DatabaseEntityProperty("提供者显示名称", Length = 500)]
        public string ProviderDisplayName { get; set; }

        [DatabaseEntityProperty("")]
        public string SnsName { get; set; }

        [DatabaseEntityProperty("")]
        public string SnsId { get; set; }

        [DatabaseEntityProperty("")]
        public string AccessToken { get; set; }

        [DatabaseEntityProperty("", Length = 1024)]
        public string IconUrl { get; set; }
    }
}
