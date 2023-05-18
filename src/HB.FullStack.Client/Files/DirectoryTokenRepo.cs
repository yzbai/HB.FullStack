/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Base;
using HB.FullStack.Client.Components.Sync;
using HB.FullStack.Common.Files;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.Files
{
    public class DirectoryTokenRepo : BaseRepo<DirectoryToken>
    {
        private readonly ILogger<DirectoryTokenRepo> _logger;
        private readonly FileManagerOptions _fileManagerOptions;
        private static readonly SemaphoreSlim _getStsTokenSemaphore = new SemaphoreSlim(1, 1);

        //TODO:有必要使用类似MonkeyCache全局Cache吗？
        private readonly Dictionary<(string, bool, string?), DirectoryToken?> _cachedDirectoryTokenDict = new Dictionary<(string, bool, string?), DirectoryToken?>();

        private readonly IDictionary<string, DirectoryPermission> _directoryPermissions = null!;

        public DirectoryTokenRepo(
            ILogger<DirectoryTokenRepo> logger,
            IOptions<FileManagerOptions> fileManagerOptions,
            IClientModelSettingFactory clientModelSettingFactory,
            IDatabase database,
            IApiClient apiClient,
            IClientEvents clientEvents,
            ITokenPreferences clientPreferences,
            ISyncManager syncManager)
            : base(logger, clientModelSettingFactory, database, apiClient, syncManager, clientEvents, clientPreferences)
        {
            _logger = logger;
            _fileManagerOptions = fileManagerOptions.Value;

            _directoryPermissions = _fileManagerOptions.DirectoryPermissions.ToDictionary(p => p.PermissionName);
        }

        #region Overrides

        protected override async Task<IEnumerable<DirectoryToken>> GetFromRemoteAsync(IApiClient apiClient, ApiRequest request)
        {
            if (request.ResName != nameof(DirectoryTokenRes))
            {
                throw ClientExceptions.UnSupportedResToModel(resName: request.ResName, modelName: nameof(DirectoryToken));
            }

            IEnumerable<DirectoryTokenRes>? resources = await apiClient.GetAsync<IEnumerable<DirectoryTokenRes>>(request);

            if (resources.IsNullOrEmpty())
            {
                return Enumerable.Empty<DirectoryToken>();
            }

            return resources.Select(Mapping.ToStsToken);
        }

        protected override Task AddToRemoteAsync(IApiClient apiClient, IEnumerable<DirectoryToken> models)
        {
            throw new NotImplementedException();
        }

        protected override Task UpdateToRemoteAsync(IApiClient apiClient, IEnumerable<PropertyChangePack> changedPacks)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteFromRemoteAsync(IApiClient apiClient, IEnumerable<DirectoryToken> models)
        {
            throw new NotImplementedException();
        }

        #endregion

        public async Task<DirectoryToken?> GetByDirectoryPermissionNameAsync(
            Guid? userId,
            string directoryPermissionName,
            bool needWritePermission,
            string? placeHolderValue,
            TransactionContext? transactionContext,
            bool remoteForced = false)
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
                placeHolderValue = userId?.ToString();
            }

            if (permission.ContainsPlaceHoder && placeHolderValue.IsNullOrEmpty())
            {
                return null;
            }

            //TODO: 这里在客户端就先检查一下，用户的UserLvel是否符合,否则到了Server端也是返回null
            //check 2 : UserLevel Check
            try
            {
                if (_cachedDirectoryTokenDict.TryGetValue((directoryPermissionName, needWritePermission, placeHolderValue), out DirectoryToken? cachedToken))
                {
                    if (cachedToken == null)
                    {
                        if (!remoteForced)
                        {
                            _logger.LogDebug("找到未过期的 AliyunStsToken缓存，为 null, directoryPermissionName: {DirectoryPermissionName}, needWrite:{NeedWritePermission}", directoryPermissionName, needWritePermission);
                            return null;
                        }
                    }
                    else
                    {
                        if (!cachedToken.IsExpired())
                        {
                            _logger.LogDebug("找到找到未过期的 AliyunStsToken缓存， directoryPermissionName: {DirectoryPermissionName}, needWrite:{NeedWritePermission}", directoryPermissionName, needWritePermission);
                            return cachedToken;
                        }
                        else
                        {
                            _logger.LogDebug("找到找到已经过期的 AliyunStsToken缓存，移除。 directoryPermissionName: {DirectoryPermissionName}, needWrite:{DirectoryPermissionName}", directoryPermissionName, needWritePermission);
                            _cachedDirectoryTokenDict.Remove((directoryPermissionName, needWritePermission, placeHolderValue));
                        }
                    }
                }

                DirectoryToken? token = await GetFirstOrDefaultAsync(
                    localWhere: token => token.UserId == userId && token.DirectoryPermissionName == directoryPermissionName,
                    remoteRequest: new DirectoryTokenResGetByDirectoryPermissionNameRequest(directoryPermissionName, placeHolderValue, !needWritePermission),
                    transactionContext: transactionContext,
                    getMode: GetSetMode.Mixed,
                    ifUseLocalData: (_, tokens) =>
                    {
                        DirectoryToken? token = tokens.FirstOrDefault();

                        return token != null && !token.IsExpired();
                    });

                _cachedDirectoryTokenDict[(directoryPermissionName, needWritePermission, placeHolderValue)] = token;

                return token;
            }
            finally
            {
                _getStsTokenSemaphore.Release();
            }
        }

        private class Mapping
        {
            public static DirectoryToken ToStsToken(DirectoryTokenRes res)
            {
                return new DirectoryToken
                {
                    UserId = res.UserId,
                    SecurityToken = res.SecurityToken,
                    AccessKeyId = res.AccessKeyId,
                    AccessKeySecret = res.AccessKeySecret,
                    ExpirationAt = res.ExpirationAt,
                    DirectoryPermissionName = res.DirectoryPermissionName,
                    ReadOnly = res.ReadOnly
                };
            }
        }
    }
}