
using HB.FullStack.Common;
using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.ModelObjects
{
    /// <summary>
    /// 通用用户类，只是登陆注册信息，不包含任何附加信息，请另行创建Profile类来存储用户其他信息
    /// </summary>
    //[Serializable]
    public class User : GuidModelObject
    {
        //对外实体，不应该包含Securitystamp
        //[Required]
        //public string SecurityStamp { get; set; } = default!;
        
        //[Password]
        //public string? PasswordHash { get; set; }

        /// <summary>
        /// "用户名称"
        /// 唯一, 可为空，一旦不为空后不可修改,注意和NickName区分,这里实为LoginName
        /// </summary>
        [LoginName]
        public string? LoginName { get; set; }
        /// <summary>
        /// "手机号",
        /// 唯一
        /// </summary>
        [Mobile]
        public string? Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }



        /// <summary>
        /// "手机号码是否验证"
        /// </summary>
        public bool MobileConfirmed { get; set; }

        /// <summary>
        /// "邮箱是否验证"
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// "Two Factor"
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
        
    }
}
