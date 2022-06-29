using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Network;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Files;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.Maui.File
{
    public class StsTokenRepo : BaseRepo<StsToken, StsTokenRes>
    {
        private readonly ILogger<StsTokenRepo> _logger;
        private readonly FileManagerOptions _fileManagerOptions;
        private static readonly SemaphoreSlim _getStsTokenSemaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<(string, bool, string?), StsToken?> _cachedAliyunStsTokenDict = new Dictionary<(string, bool, string?), StsToken?>();

        private IDictionary<string, DirectoryPermission> _directoryPermissions = null!;

        public StsTokenRepo(ILogger<StsTokenRepo> logger, IOptions<FileManagerOptions> fileManagerOptions, IDatabase database, IApiClient apiClient, IPreferenceProvider preferenceProvider, ConnectivityManager connectivityManager) : base(logger, database, apiClient, preferenceProvider, connectivityManager)
        {
            _logger = logger;
            _fileManagerOptions = fileManagerOptions.Value;

            _directoryPermissions = _fileManagerOptions.DirectoryPermissions.ToDictionary(p => p.PermissionName);
        }

        protected override StsToken ToEntity(StsTokenRes res) => StsTokenResMapper.ToStsToken(res);

        protected override StsTokenRes ToResource(StsToken entity) => StsTokenResMapper.ToStsTokenRes(entity);

        public async Task<StsToken?> GetByDirectoryPermissionNameAsync(string directoryPermissionName, bool needWritePermission, string? placeHolderValue, TransactionContext? transactionContext, bool remoteForced = false)
        {
            if (!await _getStsTokenSemaphore.WaitAsync(TimeSpan.FromSeconds(60)))
            {
                throw ClientExceptions.AliyunStsTokenOverTime(casuse: "获取AliyunStsToken失败，超出等待时间", directoryPermissionName: directoryPermissionName, needWrite: needWritePermission);
            }

            //check 1: checks
            if (!_directoryPermissions.TryGetValue(directoryPermissionName, out DirectoryPermission? permission))
            {
                return null;
            }

            if (permission.IsUserPrivate)
            {
                placeHolderValue = UserPreferences.UserId?.ToString();
            }

            if (permission.ContainsPlaceHoder && placeHolderValue.IsNullOrEmpty())
            {
                return null;
            }

            //TODO: 这里在客户端就先检查一下，用户的UserLvel是否符合,否则到了Server端也是返回null
            //check 2 : UserLevel Check

            try
            {
                if (_cachedAliyunStsTokenDict.TryGetValue((directoryPermissionName, needWritePermission, placeHolderValue), out StsToken? cachedToken))
                {
                    if (cachedToken == null)
                    {
                        if (!remoteForced)
                        {
                            _logger.LogDebug("找到未过期的 AliyunStsToken缓存，为 null, directoryPermissionName: {directoryPermissionName}, needWrite:{needWritePermission}", directoryPermissionName, needWritePermission);
                            return null;
                        }
                    }
                    else
                    {
                        if (!cachedToken.IsExpired())
                        {
                            _logger.LogDebug("找到找到未过期的 AliyunStsToken缓存， directoryPermissionName: {directoryPermissionName}, needWrite:{needWritePermission}", directoryPermissionName, needWritePermission);
                            return cachedToken;
                        }
                        else
                        {
                            _logger.LogDebug("找到找到已经过期的 AliyunStsToken缓存，移除。 directoryPermissionName: {directoryPermissionName}, needWrite:{directoryPermissionName}", directoryPermissionName, needWritePermission);
                            _cachedAliyunStsTokenDict.Remove((directoryPermissionName, needWritePermission, placeHolderValue));
                        }
                    }
                }

                Guid userId = PreferenceProvider.UserId!.Value;

                StsToken? token = await GetFirstOrDefaultAsync(
                    localWhere: token => token.UserId == userId && token.DirectoryPermissionName == directoryPermissionName,
                    remoteRequest: new StsTokenResGetByDirectoryPermissionNameRequest(ApiRequestAuth.JWT, _fileManagerOptions.AliyunStsTokenRequestUrl, directoryPermissionName, placeHolderValue),
                    transactionContext: transactionContext,
                    getMode: RepoGetMode.Mixed,
                    ifUseLocalData: (_, tokens) =>
                    {
                        StsToken? token = tokens.FirstOrDefault();

                        return token != null && !token.IsExpired();
                    });

                _cachedAliyunStsTokenDict[(directoryPermissionName, needWritePermission, placeHolderValue)] = token;

                return token;
            }
            finally
            {
                _getStsTokenSemaphore.Release();
            }
        }
    }
}