using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 通用用户类，只是登陆注册信息，不包含任何附加信息，请另行创建Profile类来存储用户其他信息
    /// </summary>
    //[Serializable]
    public class User : DatabaseEntity
    {
        [Required]
        [DatabaseEntityProperty("UserType", NotNull = true, Length = 100)]
        public string UserType { get; set; }

        //TODO: 建立索引
        [Required]
        [DatabaseEntityProperty("Guid", NotNull=true, Length=50, Unique = true)]
        public string Guid { get; set; }

        [Required]
        [DatabaseEntityProperty("SecurityStamp", NotNull = true, Length = 50)]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// 唯一, 可为空，一旦不为空后不可修改
        /// </summary>
        [UserName]
        [DatabaseEntityProperty("用户名称", Length = 100, Unique = true)]
        public string UserName { get; set; }
        /// <summary>
        /// 唯一
        /// </summary>
        [Phone]
        [DatabaseEntityProperty("手机号", Unique = true)]
        public string Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        [DatabaseEntityProperty("邮箱", Unique = true)]
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

        //[DatabaseEntityProperty("ImageUrl")]
        //public string ImageUrl { get; set; }
    }
}
