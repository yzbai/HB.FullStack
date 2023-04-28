using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using HB.FullStack.Common.Files;

using Todo.Shared.Context;

namespace Todo.Shared
{
    public static class DirectorySettings
    {
        public const string AliyunOssDirectorySeparatorChar = "/";

        /// <summary>
        /// 有哪些目录权限
        /// </summary>
        public static class DirectoryPermissions
        {
            public static readonly DirectoryPermission PUBLIC = new DirectoryPermission
            {
                PermissionName = nameof(PUBLIC),
                TopDirectory = "public",
                //Regex = "^public[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.UnRegistered), nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.UnRegistered), nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1),
            };

            public static readonly DirectoryPermission CUSTOMER = new DirectoryPermission
            {
                PermissionName = nameof(CUSTOMER),
                TopDirectory = "customer",
                //////Regex = "^customer[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1)
            };

            public static readonly DirectoryPermission CUSTOMERPRIVATE = new DirectoryPermission
            {
                PermissionName = nameof(CUSTOMERPRIVATE),
                TopDirectory = "customerprivate" + AliyunOssDirectorySeparatorChar + "{USER_ID_PLACE_HOLDER}",
                //Regex = "^customerprivate[/\\]{USER_ID_PLACE_HOLDER}[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1),
                ContainsPlaceHoder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}",
                IsUserPrivate = true
            };

            public static IList<DirectoryPermission> All { get; } = new List<DirectoryPermission>
            {
                PUBLIC,
                CUSTOMER,
                CUSTOMERPRIVATE
            };
        }

        /// <summary>
        /// 有哪些具体的目录，分别需要使用哪个权限
        /// </summary>
        public static class Descriptions
        {
            //TODO: 修改这些ExpiryTime
            public static readonly DirectoryDescription PUBLIC = new DirectoryDescription
            {
                DirectoryName = nameof(PUBLIC),
                DirectoryPath = "public",
                DirectoryPermissionName = DirectoryPermissions.PUBLIC.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription PUBLIC_AVATAR = new DirectoryDescription
            {
                DirectoryName = nameof(PUBLIC_AVATAR),
                DirectoryPath = "public" + Path.DirectorySeparatorChar + "avator",
                DirectoryPermissionName = DirectoryPermissions.PUBLIC.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription CUSTOMER = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMER),
                DirectoryPath = "customer",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription CUSTOMER_TEMP = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMER_TEMP),
                DirectoryPath = "customer" + Path.DirectorySeparatorChar + "temp",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription SYSTEM = new DirectoryDescription
            {
                DirectoryName = nameof(SYSTEM),
                DirectoryPath = "system",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription SYSTEM_THEME = new DirectoryDescription
            {
                DirectoryName = nameof(SYSTEM_THEME),
                DirectoryPath = "system" + Path.DirectorySeparatorChar + "theme",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription CUSTOMERPRIVATE = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMERPRIVATE),
                DirectoryPath = "customerprivate" + Path.DirectorySeparatorChar + "{USER_ID_PLACE_HOLDER}",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMERPRIVATE.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1),
                IsPathContainsPlaceHolder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}"
            };
            public static readonly DirectoryDescription CUSTOMERPRIVATE_TEMP = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMERPRIVATE_TEMP),
                DirectoryPath = "customerprivate" + Path.DirectorySeparatorChar + "{USER_ID_PLACE_HOLDER}" + Path.DirectorySeparatorChar + "temp",
                DirectoryPermissionName = DirectoryPermissions.CUSTOMERPRIVATE.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1),
                IsPathContainsPlaceHolder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}"
            };
            public static readonly IList<DirectoryDescription> All = new List<DirectoryDescription>
            {
                PUBLIC,
                PUBLIC_AVATAR,

                CUSTOMER,
                CUSTOMER_TEMP,

                SYSTEM,
                SYSTEM_THEME,

                CUSTOMERPRIVATE,
                CUSTOMERPRIVATE_TEMP
            };
        }

        #region Directories - for easy use

        public static readonly Directory2 PUBLIC = Descriptions.PUBLIC.ToDirectory(null);
        public static readonly Directory2 PUBLIC_AVATAR = Descriptions.PUBLIC_AVATAR.ToDirectory(null);
        public static readonly Directory2 CUSTOMER = Descriptions.CUSTOMER.ToDirectory(null);
        public static readonly Directory2 CUSTOMER_TEMP = Descriptions.CUSTOMER_TEMP.ToDirectory(null);
        public static readonly Directory2 SYSTEM = Descriptions.SYSTEM.ToDirectory(null);
        public static readonly Directory2 SYSTEM_THEME = Descriptions.SYSTEM_THEME.ToDirectory(null);
        public static Directory2 CUSTOMERPRIVATE(Guid? userId) => Descriptions.CUSTOMERPRIVATE.ToDirectory(userId?.ToString());
        public static Directory2 CUSTOMERPRIVATE_TEMP(Guid? userId) => Descriptions.CUSTOMERPRIVATE_TEMP.ToDirectory(userId?.ToString());

        #endregion
    }
}
