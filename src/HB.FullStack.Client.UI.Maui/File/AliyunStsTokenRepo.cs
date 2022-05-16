using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Network;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.UI.Maui.File
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
                DirectoryRegExp = obj.DirectoryRegExp,
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

    public class AliyunStsTokenRepo : BaseRepo<AliyunStsToken, AliyunStsTokenRes>
    {
        private readonly ILogger<AliyunStsTokenRepo> _logger;
        private readonly FileManagerOptions _fileManagerOptions;
        private static readonly SemaphoreSlim _getStsTokenSemaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<(string, bool), AliyunStsToken?> _cachedDirectoryTokenDict = new Dictionary<(string, bool), AliyunStsToken?>();

        public AliyunStsTokenRepo(ILogger<AliyunStsTokenRepo> logger, IOptions<FileManagerOptions> fileManagerOptions, IDatabase database, IApiClient apiClient, IPreferenceProvider preferenceProvider, ConnectivityManager connectivityManager) : base(logger, database, apiClient, preferenceProvider, connectivityManager)
        {
            _logger = logger;
            _fileManagerOptions = fileManagerOptions.Value;
        }

        protected override AliyunStsToken ToEntity(AliyunStsTokenRes res) => AliyunStsTokenResMapper.ToAliyunStsToken(res);

        protected override AliyunStsTokenRes ToResource(AliyunStsToken entity) => AliyunStsTokenResMapper.ToAliyunStsTokenRes(entity);

        /// <summary>
        /// 得到与requestDirectory匹配的最大权限的Token
        /// </summary>
        public async Task<AliyunStsToken?> GetByDirectoryAsync(string requestDirectory, bool needWritePermission, TransactionContext? transactionContext, bool remoteForced = false)
        {
            if (!await _getStsTokenSemaphore.WaitAsync(TimeSpan.FromSeconds(60)).ConfigureAwait(false))
            {
                throw ClientExceptions.AliyunStsTokenOverTime(casuse: "获取AliyunStsToken失败，超出等待时间", requestDirectory: requestDirectory, needWrite: needWritePermission);
            }

            try
            {
                if (_cachedDirectoryTokenDict.TryGetValue((requestDirectory, needWritePermission), out AliyunStsToken? cachedToken))
                {
                    if (cachedToken == null)
                    {
                        if (!remoteForced)
                        {
                            _logger.LogDebug("找到未过期的 AliyunStsToken缓存，为 null, requestDirectory: {requestDirectory}, needWrite:{needWritePermission}", requestDirectory, needWritePermission);
                            return null;
                        }
                    }
                    else
                    {
                        if (cachedToken.ExpirationAt - TimeUtil.UtcNow >= TimeSpan.FromMinutes(1))
                        {
                            _logger.LogDebug("找到找到未过期的 AliyunStsToken缓存， requestDirectory: {requestDirectory}, needWrite:{needWritePermission}", requestDirectory, needWritePermission);
                            return cachedToken;
                        }
                        else
                        {
                            _logger.LogDebug("找到找到已经过期的 AliyunStsToken缓存，移除。 requestDirectory: {requestDirectory}, needWrite:{needWritePermission}", requestDirectory, needWritePermission);
                            _cachedDirectoryTokenDict.Remove((requestDirectory, needWritePermission));
                        }
                    }
                }

                Guid userId = PreferenceProvider.UserId!.Value;

                IEnumerable<AliyunStsToken> tokens = await GetAsync(
                    where: token => token.UserId == userId /* && requestDirectory.StartsWith(token.Directory, GlobalSettings.ComparisonIgnoreCase)*/,
                    request: new AliyunStsTokenResGetByDirectoryRequest(requestDirectory, _fileManagerOptions.AliyunStsTokenRequestUrl),
                    transactionContext: transactionContext,
                    getMode: RepoGetMode.Mixed,
                    whetherUseLocalData: (_, entities) =>
                    {
                        if (entities.IsNullOrEmpty())
                        {
                            return false;
                        }

                        List<AliyunStsToken> machedTokens = FindMatches(requestDirectory, needWritePermission, entities);

                        if (!machedTokens.Any())
                        {
                            return false;
                        }

                        foreach (AliyunStsToken token in machedTokens)
                        {
                            if (token.ExpirationAt - TimeUtil.UtcNow < TimeSpan.FromMinutes(1))
                            {
                                return false;
                            }
                        }

                        return true;
                    }).ConfigureAwait(false);

                var machedTokens = FindMatches(requestDirectory, needWritePermission, tokens);

                AliyunStsToken? matchedToken = machedTokens.OrderBy(t => t.DirectoryRegExp.Length).FirstOrDefault();

                _cachedDirectoryTokenDict[(requestDirectory, needWritePermission)] = matchedToken;

                _logger.LogDebug("得到 AliyunStsToken缓存, requestDirectory: {requestDirectory}, needWrite:{needWritePermission}", requestDirectory, needWritePermission);

                return matchedToken;
            }
            finally
            {
                _getStsTokenSemaphore.Release();
            }
        }

        private static List<AliyunStsToken> FindMatches(string requestDirectory, bool needWritePermission, IEnumerable<AliyunStsToken> entities)
        {
            var machedTokens = entities.Where(t => Regex.IsMatch(requestDirectory, t.DirectoryRegExp, RegexOptions.IgnoreCase));

            if (needWritePermission)
            {
                return machedTokens.Where(t => t.ReadOnly == false).ToList();
            }

            return machedTokens.ToList();
        }
    }
}