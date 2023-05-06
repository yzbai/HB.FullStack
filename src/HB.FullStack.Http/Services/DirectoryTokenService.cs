using System;
using System.Collections.Generic;

using HB.FullStack.Common.Files;
using HB.Infrastructure.Aliyun.Sts;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.WebLib.Services
{
    public class DirectoryTokenService : IDirectoryTokenService
    {
        private readonly IAliyunStsService _aliyunStsService;
        private readonly DirectoryOptions _directoryOptions;

        private readonly Dictionary<string, DirectoryPermission> _directoryPermissionDict = new Dictionary<string, DirectoryPermission>();

        public DirectoryTokenService(IAliyunStsService aliyunStsService, IOptions<DirectoryOptions> directoryOptions)
        {
            _aliyunStsService = aliyunStsService;
            _directoryOptions = directoryOptions.Value;

            RangeDirectoryPermissions();

            void RangeDirectoryPermissions()
            {
                foreach (DirectoryPermission permission in _directoryOptions.DirectoryPermissions)
                {
                    foreach (string userLevel in permission.ReadUserLevels)
                    {
                        _directoryPermissionDict[GetDirectoryPermissionKey(userLevel, permission.PermissionName, true)] = permission;
                    }

                    foreach (string userLevel in permission.ReadUserLevels)
                    {
                        _directoryPermissionDict[GetDirectoryPermissionKey(userLevel, permission.PermissionName, false)] = permission;
                    }
                }
            }
        }

        private static string GetDirectoryPermissionKey(string? userLevel, string directoryPermissionName, bool read)
        {
            return $"{userLevel ?? ""}_{directoryPermissionName}_{read}";
        }

        public DirectoryToken? GetDirectoryToken(Guid requestUserId, string? userLevel, string directoryPermissionName, string? placeHolderValue, bool readOnly)
        {
            string permissionKey = GetDirectoryPermissionKey(userLevel, directoryPermissionName, readOnly);

            if (!_directoryPermissionDict.TryGetValue(permissionKey, out DirectoryPermission? permission))
            {
                return null;
            }

            if (permission.ContainsPlaceHoder && placeHolderValue.IsNullOrEmpty())
            {
                return null;
            }

            if (permission.IsUserPrivate && requestUserId.ToString() != placeHolderValue)
            {
                //个人文件夹权限
                //TODO: Throw Exception or log
                return null;
            }

            string requestDirectory = permission.TopDirectory;

            if (permission.ContainsPlaceHoder)
            {
                requestDirectory = requestDirectory.Replace(permission.PlaceHolderName!, placeHolderValue, StringComparison.Ordinal);
            }

            StsToken? token = _aliyunStsService.RequestOssStsToken(requestUserId, _directoryOptions.AliyunOssBucketName, requestDirectory, readOnly, permission.ExpiryTime.TotalSeconds);

            if (token == null)
            {
                return null;
            }

            DirectoryToken directoryToken = ToDirectoryToken(token);

            directoryToken.DirectoryPermissionName = permission.PermissionName;
            directoryToken.UserId = requestUserId;

            return directoryToken;
        }

        private static DirectoryToken ToDirectoryToken(StsToken stsToken)
        {
            return new DirectoryToken
            {
                SecurityToken = stsToken.SecurityToken,
                AccessKeyId = stsToken.AccessKeyId,
                AccessKeySecret = stsToken.AccessKeySecret,
                ExpirationAt = stsToken.ExpirationAt,
                ReadOnly = stsToken.ReadOnly
            };
        }
    }
}
