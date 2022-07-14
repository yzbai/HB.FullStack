using HB.FullStack.Database.DatabaseModels;

using System;
using System.ComponentModel.DataAnnotations;

using static HB.FullStack.Identity.LengthConventions;

namespace HB.FullStack.Identity.Models
{
    /// <summary>
    /// 通用用户类，只是登陆注册信息，不包含任何附加信息，请另行创建Profile类来存储用户其他信息
    /// </summary>
    public class User : TimestampGuidDBModel
    {
        [Required]
        [Guid32String(NotNull = true)]
        public string SecurityStamp { get; set; } = default!;

        [Password]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// "用户名称"
        /// 唯一, 可为空，一旦不为空后不可修改,注意和NickName区分,这里实为LoginName
        /// </summary>
        [LoginName]
        [DatabaseModelProperty(MaxLength = MAX_USER_LOGIN_NAME_LENGTH, Unique = true)]
        public string? LoginName { get; set; }

        /// <summary>
        /// "手机号",
        /// 唯一
        /// </summary>
        [Mobile]
        [DatabaseModelProperty(MaxLength = MAX_USER_MOBILE_LENGTH, Unique = true)]
        public string? Mobile { get; set; }

        /// <summary>
        /// 唯一，可为空
        /// </summary>
        [EmailAddress]
        [DatabaseModelProperty(MaxLength = MAX_USER_EMAIL_LENGTH, Unique = true)]
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

        public User()
        {
        }

        public User(string? loginName, string? mobile, string? email, string? password, bool mobileConfirmed, bool emailConfirmed)
        {
            SecurityStamp = SecurityUtil.CreateUniqueToken();
            LoginName = loginName;
            Mobile = mobile;
            Email = email;
            PasswordHash = password == null ? null : SecurityUtil.EncryptPwdWithSalt(password, SecurityStamp);
            MobileConfirmed = mobileConfirmed;
            EmailConfirmed = emailConfirmed;
        }
    }
}