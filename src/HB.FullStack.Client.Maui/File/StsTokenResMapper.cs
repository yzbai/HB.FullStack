using HB.FullStack.Common.ApiClient;

namespace HB.FullStack.Client.Maui.File
{
    public static class StsTokenResMapper
    {
        public static StsToken ToStsToken(StsTokenRes obj)
        {
            return new StsToken
            {
                Id = obj.Id,
                Version = obj.Version,
                LastTime = obj.LastTime,
                LastUser = obj.LastUser,

                UserId = obj.UserId,
                SecurityToken = obj.SecurityToken,
                AccessKeyId = obj.AccessKeyId,
                AccessKeySecret = obj.AccessKeySecret,
                ExpirationAt = obj.ExpirationAt,
                DirectoryPermissionName = obj.DirectoryPermissionName,
                ReadOnly = obj.ReadOnly
            };
        }

        public static StsTokenRes ToStsTokenRes(StsToken obj)
        {
            return new StsTokenRes
            {
                Id = obj.Id,
                Version = obj.Version,
                LastTime = obj.LastTime,
                LastUser = obj.LastUser,

                UserId = obj.UserId,
                SecurityToken = obj.SecurityToken,
                AccessKeyId = obj.AccessKeyId,
                AccessKeySecret = obj.AccessKeySecret,
                ExpirationAt = obj.ExpirationAt,
                DirectoryPermissionName = obj.DirectoryPermissionName,
                ReadOnly = obj.ReadOnly
            };
        }
    }
}