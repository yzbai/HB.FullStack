using HB.Framework.Database.Entity;
using HB.Component.Identity;
using System;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Entity
{
    public class ThirdPartyLogin : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [ForeignKey(typeof(User))]
        [GuidEntityProperty]
        public string UserGuid { get; set; }

        [EntityProperty("登陆提供者", Length=500)]
        public string LoginProvider { get; set; }

        [EntityProperty("登陆key", Length=500)]
        public string ProviderKey { get; set; }

        [EntityProperty("提供者显示名称", Length = 500)]
        public string ProviderDisplayName { get; set; }

        [EntityProperty("")]
        public string SnsName { get; set; }

        [EntityProperty("")]
        public string SnsId { get; set; }

        [EntityProperty("")]
        public string AccessToken { get; set; }

        [EntityProperty("", Length = 1024)]
        public string IconUrl { get; set; }
    }
}
