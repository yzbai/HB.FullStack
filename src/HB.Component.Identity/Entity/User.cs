using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 通用用户类
    /// </summary>
    //[Serializable]
    public class User : DatabaseEntity
    {
        [Required]
        [DatabaseEntityProperty("Guid", NotNull=true, Length=50)]
        public string Guid { get; set; }

        [Required]
        [DatabaseEntityProperty("SecurityStamp", NotNull = true, Length = 50)]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// 唯一, 可为空，一旦不为空后不可修改
        /// </summary>
        [UserName]
        [DatabaseEntityProperty("用户名称", Length = 100)]
        public string UserName { get; set; }
        /// <summary>
        /// 唯一
        /// </summary>
        [Phone]
        [DatabaseEntityProperty("手机号")]
        public string Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        [DatabaseEntityProperty("邮箱")]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Password]
        [DatabaseEntityProperty("密码")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 未激活，可以进行注册，潜在用户
        /// </summary>
        [DatabaseEntityProperty("潜在用户")]
        public bool IsActivated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("手机号码是否验证")]
        public bool MobileConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("邮箱是否验证")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("Two Factor")]
        public bool TwoFactorEnabled { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("Lockout enabled")]
        public bool LockoutEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("LockendDatae")]
        public DateTimeOffset? LockoutEndDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DatabaseEntityProperty("Accessfailed count")]
        public long AccessFailedCount { get; set; }

        [DatabaseEntityProperty("Accessfailed last time")]
        public DateTimeOffset? AccessFailedLastTime { get; set; }

        [DatabaseEntityProperty("ImageUrl")]
        public string ImageUrl { get; set; }
    }
}
