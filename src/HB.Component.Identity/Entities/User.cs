using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.Entities
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
        /// "用户名称"
        /// 唯一, 可为空，一旦不为空后不可修改,注意和NickName区分,这里实为LoginName
        /// </summary>
        [LoginName]
        [EntityProperty(MaxLength = 100, Unique = true)]
        public string? LoginName { get; set; }
        /// <summary>
        /// "手机号",
        /// 唯一
        /// </summary>
        [Mobile]
        [EntityProperty(Unique = true, MaxLength = 14)]
        public string? Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        [EntityProperty(Unique = true, MaxLength = 256)]
        public string? Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Password]
        [EntityProperty]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// "手机号码是否验证"
        /// </summary>
        [EntityProperty]
        public bool MobileConfirmed { get; set; }

        /// <summary>
        /// "邮箱是否验证"
        /// </summary>
        [EntityProperty]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// "Two Factor"
        /// </summary>
        [EntityProperty]
        public bool TwoFactorEnabled { get; set; }
    }
}
