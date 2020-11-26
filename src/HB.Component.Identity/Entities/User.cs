using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Identity.Entities
{
    /// <summary>
    /// 通用用户类，只是登陆注册信息，不包含任何附加信息，请另行创建Profile类来存储用户其他信息
    /// </summary>
    //[Serializable]
    [DatabaseEntity]
    public class User : Entity
    {
        [Required]
        [GuidEntityProperty(NotNull = true)]
        public string SecurityStamp { get; set; } = default!;

        /// <summary>
        /// 唯一, 可为空，一旦不为空后不可修改,注意和NickName区分,这里实为LoginName
        /// </summary>
        [LoginName]
        [EntityProperty("用户名称", Length = 100, Unique = true)]
        public string? LoginName { get; set; }
        /// <summary>
        /// 唯一
        /// </summary>
        [Mobile]
        [EntityProperty("手机号", Unique = true, Length = 14)]
        public string? Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        [EntityProperty("邮箱", Unique = true, Length = 256)]
        public string? Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Password]
        [EntityProperty("密码")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EntityProperty("手机号码是否验证")]
        public bool MobileConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EntityProperty("邮箱是否验证")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EntityProperty("Two Factor")]
        public bool TwoFactorEnabled { get; set; }
    }
}
