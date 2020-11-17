using HB.Framework.Database.Entities;
using HB.Component.Identity;
using System;
using HB.Component.Identity.Entities;
using HB.Framework.Common.Entities;

namespace HB.Component.Authorization.Entities
{
    [DatabaseEntity]
    public class ThirdPartyLogin : Entity
    {
        [ForeignKey(typeof(IdentityUser))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [EntityProperty("登陆提供者", Length = 500, NotNull = true)]
        public string LoginProvider { get; set; } = default!;

        [EntityProperty("登陆key", Length = 500, NotNull = true)]
        public string ProviderKey { get; set; } = default!;

        [EntityProperty("提供者显示名称", Length = 500, NotNull = true)]
        public string ProviderDisplayName { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string SnsName { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string SnsId { get; set; } = default!;

        [EntityProperty("", NotNull = true)]
        public string AccessToken { get; set; } = default!;

        [EntityProperty("", Length = 1024)]
        public string? IconAddress { get; set; }
    }
}
