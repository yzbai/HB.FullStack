using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Network;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Files;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.Maui.File
{
    public class AliyunStsTokenRepo : BaseRepo<AliyunStsToken, AliyunStsTokenRes>
    {
        private readonly ILogger<AliyunStsTokenRepo> _logger;
        private readonly FileManagerOptions _fileManagerOptions;
        private static readonly SemaphoreSlim _getStsTokenSemaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<(string, bool), AliyunStsToken?> _cachedAliyunStsTokenDict = new Dictionary<(string, bool), AliyunStsToken?>();

        private IDictionary<string, DirectoryPermission> _directoryPermissions = null!;
        private IDictionary<string, DirectoryDescription> _directoryDescriptions = null!;

        public AliyunStsTokenRepo(ILogger<AliyunStsTokenRepo> logger, IOptions<FileManagerOptions> fileManagerOptions, IDatabase database, IApiClient apiClient, IPreferenceProvider preferenceProvider, ConnectivityManager connectivityManager) : base(logger, database, apiClient, preferenceProvider, connectivityManager)
        {
            _logger = logger;
            _fileManagerOptions = fileManagerOptions.Value;

            _directoryPermissions = _fileManagerOptions.DirectoryPermissions.ToDictionary(p => p.PermissionName);
            _directoryDescriptions = _fileManagerOptions.Directories.ToDictionary(p => p.DirectoryName);
        }

        protected override AliyunStsToken ToEntity(AliyunStsTokenRes res) => AliyunStsTokenResMapper.ToAliyunStsToken(res);

        protected override AliyunStsTokenRes ToResource(AliyunStsToken entity) => AliyunStsTokenResMapper.ToAliyunStsTokenRes(entity);

        public async Task<AliyunStsToken?> GetByDirectoryPermissionNameAsync(string directoryPermissionName, bool needWritePermission, TransactionContext? transactionContext, bool remoteForced = false)
        {
            if (!await _getStsTokenSemaphore.WaitAsync(TimeSpan.FromSeconds(60)))
            {
                throw ClientExceptions.AliyunStsTokenOverTime(casuse: "获取AliyunStsToken失败，超出等待时间", directoryPermissionName: directoryPermissionName, needWrite: needWritePermission);
            }

            //check 1: Exists check
            if (!_directoryPermissions.TryGetValue(directoryPermissionName, out DirectoryPermission? permissions))
            {
                return null;
            }

            //TODO: 这里在客户端就先检查一下，用户的UserLvel是否符合,否则到了Server端也是返回null
            //check 2 : UserLevel Check

            try
            {
                if (_cachedAliyunStsTokenDict.TryGetValue((directoryPermissionName, needWritePermission), out AliyunStsToken? cachedToken))
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
                            _cachedAliyunStsTokenDict.Remove((directoryPermissionName, needWritePermission));
                        }
                    }
                }

                Guid userId = PreferenceProvider.UserId!.Value;

                AliyunStsToken? token = await GetFirstOrDefaultAsync(
                    where: token => token.UserId == userId && token.DirectoryPermissionName == directoryPermissionName,
                    request: new AliyunStsTokenResGetByDirectoryPermissionNameRequest(directoryPermissionName, _fileManagerOptions.AliyunStsTokenRequestUrl),
                    transactionContext: transactionContext,
                    getMode: RepoGetMode.Mixed,
                    ifUseLocalData: (_, tokens) =>
                    {
                        AliyunStsToken? token = tokens.FirstOrDefault();

                        if (token == null)
                        {
                            return false;
                        }

                        return !token.IsExpired();
                    });

                _cachedAliyunStsTokenDict[(directoryPermissionName, needWritePermission)] = token;

                return token;
            }
            finally
            {
                _getStsTokenSemaphore.Release();
            }
        }
    }
}