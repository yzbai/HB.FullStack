/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aliyun.OSS;
using Aliyun.OSS.Common;
using Aliyun.OSS.Common.Authentication;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.Components.KVManager;
using HB.FullStack.Client.Components.Sts;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.Components.Files
{
    public partial class FileManager : IFileManager
    {
        private readonly ILogger _logger;
        private readonly ILocalFileManager _localFileManager;
        private readonly ITokenPreferences _clientPreferences;
        private readonly IDbSimpleLocker _dbLocker;
        private readonly StsTokenRepo _aliyunStsTokenRepo;
        private readonly FileManagerOptions _options;
        private readonly Dictionary<string, DirectoryDescription> _directories;
        private readonly ObjectPool<IOss> _ossPool;

        public FileManager(
            IOptions<FileManagerOptions> options,
            ILogger<FileManager> logger,
            ILocalFileManager localFileManager,
            ITokenPreferences preferenceProvider,
            IDbSimpleLocker dbLocker,
            StsTokenRepo aliyunStsTokenRepo)
        {
            _options = options.Value;
            _logger = logger;
            _localFileManager = localFileManager;
            _clientPreferences = preferenceProvider;
            _dbLocker = dbLocker;
            _aliyunStsTokenRepo = aliyunStsTokenRepo;

            OssClientPoolPolicy poolPolicy = new OssClientPoolPolicy(_options.AliyunOssEndpoint);
            _ossPool = new DefaultObjectPool<IOss>(poolPolicy, 4);

            _directories = _options.DirectoryDescriptions.ToDictionary(d => d.DirectoryName);
        }

        private async Task<IOss> RentOssClientAsync(string directoryPermissionName, bool needWrite, string? placeHolderValue, bool recheckPermissionForced = false)
        {
            StsToken? stsToken = await _aliyunStsTokenRepo.GetByDirectoryPermissionNameAsync(
                _clientPreferences.UserId,
                directoryPermissionName,
                needWrite,
                placeHolderValue,
                null,
                recheckPermissionForced);

            if (stsToken == null)
            {
                _logger.LogDebug("得到空的 AliyunStsToken, Upload Avatar");
                throw ClientExceptions.AliyunStsTokenReturnNull();
            }

            //TODO: 可以进一步抽象，把OssProvider做出来,或者独立出HB.FullStack.Client.FileManager.Aliyun

            IOss oss = _ossPool.Get();

            oss.SwitchCredentials(new DefaultCredentials(stsToken.AccessKeyId, stsToken.AccessKeySecret, stsToken.SecurityToken));

            return oss;
        }

        private void ReturnOssClient(IOss? oss)
        {
            if (oss != null)
            {
                _ossPool.Return(oss);
            }
        }

        private DirectoryDescription GetDirectoryDescription(Directory2 directory)
        {
            if (_directories.TryGetValue(directory.DirectoryName, out DirectoryDescription? directoryDescription))
            {
                return directoryDescription;
            }

            throw ClientExceptions.NoSuchDirectory(directory.DirectoryName);
        }

        ///<summary>
        ///返回本地fullpath
        ///</summary>
        public async Task<string> SetAsync(string sourceLocalFullPath, Directory2 directory, string fileName, bool recheckPermissionForced = false)
        {
            //TODO: 先检查网络连接

            //if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            //{
            //    throw Exceptions.NoInternet();
            //}

            //首先拷贝到本地
            string localFullPath = _localFileManager.GetFullPath(directory, fileName);

            if (!localFullPath.Equals(sourceLocalFullPath, StringComparison.OrdinalIgnoreCase))
            {
                using Stream stream = System.IO.File.Open(sourceLocalFullPath, FileMode.Open);
                _ = await _localFileManager.SaveFileAsync(stream, localFullPath);
            }

            DirectoryDescription description = GetDirectoryDescription(directory);

            //upload到远程
            //TODO: 检查file extension，以确保符合destDirectory要求，Avatar 允许图片类型
            //可参考FileUtil对文件的检查
            IOss oss = await RentOssClientAsync(description.DirectoryPermissionName, true, directory.PlaceHolderValue, recheckPermissionForced);

            try
            {
                string ossKey = GetOssKey(directory, fileName);

                PutObjectResult result = oss.PutObject(_options.AliyunOssBucketName, ossKey, localFullPath);

                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    //TODO:  处理错误
                    //https://help.aliyun.com/document_detail/31978.html?spm=a2c4g.11186623.6.1679.620514a04pvo6I
                    throw ClientExceptions.AliyunOssPutObjectError(bucketName: _options.AliyunOssBucketName, key: ossKey);
                }

                await LockNewlyAddedFile(ossKey, description.ExpiryTime);
            }
            catch (OssException ex)
            {
                string message = $"Oss上传官方错误， ErrorCode:{ex.ErrorCode}, Message:{ex.Message}";

                _logger.LogError(ex, message);

                throw ClientExceptions.AliyunOssPutObjectError(cause: message, null);
            }
            catch (Exception ex)
            {
                throw ClientExceptions.AliyunOssPutObjectError(cause: "Oss上传其他错误", innerException: ex);
            }
            finally
            {
                ReturnOssClient(oss);
            }

            return localFullPath;
        }

        /// <summary>
        /// 返回本地FullPath
        /// </summary>
        public async Task<string> GetAsync(Directory2 directory, string fileName, bool remoteForced = false)
        {
            string ossKey = GetOssKey(directory, fileName);
            DirectoryDescription description = GetDirectoryDescription(directory);
            TimeSpan localFileExpiryTime = description.ExpiryTime;

            //刚请求过,且存在就返回
            if (!remoteForced && !await _dbLocker.NoWaitLockAsync(nameof(FileManager), ossKey, localFileExpiryTime))
            {
                Globals.Logger.LogDebug("前不久请求过文件 {Osskey}", ossKey);

                //TODO: 思考，这里有一种，第一个线程已经取锁成功，去下载图片，还没下完，第二个进来一看，以为已经取好了，结果没有
                string fullPath = _localFileManager.GetFullPath(directory, fileName);

                int tryCount = 3;

                while (tryCount-- > 0)
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        return fullPath;
                    }

                    Globals.Logger.LogDebug("前不久请求过文件，现在还不存在，等待 1秒，再尝试");
                    await Task.Delay(100);
                }

                Globals.Logger.LogDebug("前不久请求过文件，现在还不存在，不再等待，远程获取");
            }

            IOss oss = null!;
            try
            {
                oss = await RentOssClientAsync(description.DirectoryPermissionName, false, directory.PlaceHolderValue, remoteForced);

                using OssObject ossObject = oss.GetObject(_options.AliyunOssBucketName, ossKey);

                //覆盖本地
                string localFullPath = await _localFileManager.SaveFileAsync(ossObject.Content, directory, fileName);

                Globals.Logger.LogDebug("远程文件获取成功，已经保存到本地 {Osskey}", ossKey);

                return localFullPath;
            }
            catch (Exception ex)
            {
                //TODO:  处理这里, HttpRequrestException
                throw ClientExceptions.FileServiceError(fileName: fileName, directoryName: directory.DirectoryName, cause: "Oss获取文件出错", innerException: ex);
            }
            finally
            {
                ReturnOssClient(oss);
            }
        }

        private async Task LockNewlyAddedFile(string ossKey, TimeSpan expiryTime)
        {
            await _dbLocker.NoWaitLockAsync(nameof(FileManager), ossKey, expiryTime);
        }

        /// <summary>

        private string GetOssKey(Directory2 directory, string fileName)
        {
            DirectoryDescription description = GetDirectoryDescription(directory);

            string directoryPath = description.GetPath(directory.PlaceHolderValue);

            if (Path.DirectorySeparatorChar == '\\')
            {
                directoryPath = directoryPath.Replace('\\', '/');
            }

            return $"{directoryPath}/{fileName}";
        }

        private class OssClientPoolPolicy : IPooledObjectPolicy<IOss>
        {
            private readonly string _endPoint;

            public OssClientPoolPolicy(string endPoint)
            {
                _endPoint = endPoint;
            }

            public IOss Create()
            {
                //TODO:change accesskey
                return new OssClient(_endPoint, "will_changed_later", "will_changed_later");
            }

            public bool Return(IOss obj)
            {
                return true;
            }
        }

        public ILocalFileManager LocalFileManager => _localFileManager;
    }
}