namespace HB.FullStack.Client.Maui.File
{
    public static class AliyunStsTokenResMapper
    {
        public static AliyunStsToken ToAliyunStsToken(AliyunStsTokenRes obj)
        {
            return new AliyunStsToken
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
                DirectoryPermissionName = obj.DirectoryRegExp,
                ReadOnly = obj.ReadOnly
            };
        }

        public static AliyunStsTokenRes ToAliyunStsTokenRes(AliyunStsToken obj)
        {
            return new AliyunStsTokenRes
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
                DirectoryRegExp = obj.DirectoryRegExp,
                ReadOnly = obj.ReadOnly
            };
        }
    }
}