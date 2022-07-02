using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aliyun.OSS;
using Aliyun.OSS.Common;
using Aliyun.OSS.Common.Authentication;

using HB.FullStack.Client.File;
using HB.FullStack.Client.KeyValue;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace HB.FullStack.Client.Maui.File
{
    public partial class FileManager : IFileManager
    {
        private readonly ILogger _logger;
        private readonly IPreferenceProvider _preferenceProvider;
        private readonly DbSimpleLocker _dbLocker;
        private readonly StsTokenRepo _aliyunStsTokenRepo;
        private readonly FileManagerOptions _options;
        private Dictionary<string, DirectoryDescription> _directories;
        private readonly ObjectPool<IOss> _ossPool;

        public FileManager(ILogger<FileManager> logger, IOptions<FileManagerOptions> options, IPreferenceProvider preferenceProvider, DbSimpleLocker dbLocker, StsTokenRepo aliyunStsTokenRepo)
        {
            _options = options.Value;
            _logger = logger;
            _preferenceProvider = preferenceProvider;
            _dbLocker = dbLocker;
            _aliyunStsTokenRepo = aliyunStsTokenRepo;

            OssClientPoolPolicy poolPolicy = new OssClientPoolPolicy(_options.AliyunOssEndpoint);
            _ossPool = new DefaultObjectPool<IOss>(poolPolicy, 4);

            _directories = _options.DirectoryDescriptions.ToDictionary(d => d.DirectoryName);
        }

        private async Task<IOss> RentOssClientAsync(string directoryPermissionName, bool needWrite, string? placeHolderValue, bool recheckPermissionForced = false)
        {
            StsToken? stsToken = await _aliyunStsTokenRepo.GetByDirectoryPermissionNameAsync(directoryPermissionName, needWrite, placeHolderValue, null, recheckPermissionForced);

            if (stsToken == null)
            {
                _logger.LogDebug("得到空的 AliyunStsToken, Upload Avatar");
                throw Exceptions.AliyunStsTokenReturnNull();
            }

            //TODO: 可以进一步抽象，把OssProvider做出来

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

        public static string PathRoot { get; } = FileSystem.AppDataDirectory;

        private DirectoryDescription GetDirectoryDescription(Directory2 directory)
        {
            if(_directories.TryGetValue(directory.DirectoryName, out DirectoryDescription? directoryDescription))
            {
                return directoryDescription;
            }

            throw ClientExceptions.NoSuchDirectory(directory.DirectoryName);
        }

        [return: NotNullIfNotNull("fileName")]
        public string? GetLocalFullPath(Directory2 directory, string fileName)
        {
            DirectoryDescription description = GetDirectoryDescription(directory);

            if (description.IsPathContainsPlaceHolder && directory.PlaceHolderValue.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(directory.PlaceHolderValue));
            }

            return Path.Combine(PathRoot, description.GetPath(directory.PlaceHolderValue), fileName);
        }

        public string GetNewTempFullPath(string fileExtension)
        {
            string tempFileName = GetRandomFileName(fileExtension);
            //return GetLocalFullPath(CurrentUserTempDirectory, tempFileName);
            return Path.Combine(PathRoot, "temp", tempFileName);
        }

        ///<summary>
        ///返回本地fullpath
        ///</summary>
        public async Task<string> SetFileToMixedAsync(string sourceLocalFullPath, Directory2 directory, string fileName, bool recheckPermissionForced = false)
        {
            //TODO: 先检查网络连接

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                throw Exceptions.NoInternet();
            }

            //首先拷贝到本地
            string localFullPath = GetLocalFullPath(directory, fileName);

            if (!localFullPath.Equals(sourceLocalFullPath, StringComparison.OrdinalIgnoreCase))
            {
                using (Stream stream = System.IO.File.Open(sourceLocalFullPath, FileMode.Open))
                {
                    _ = await SaveFileToLocalAsync(stream, localFullPath);
                }
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
                    throw Exceptions.AliyunOssPutObjectError(bucketName: _options.AliyunOssBucketName, key: ossKey);
                }

                await LockNewlyAddedFile(ossKey, description.ExpiryTime);
            }
            catch (OssException ex)
            {
                string message = $"Oss上传官方错误， ErrorCode:{ex.ErrorCode}, Message:{ex.Message}";

                _logger.LogError(ex, message);

                throw Exceptions.AliyunOssPutObjectError(cause: message, null);
            }
            catch (Exception ex)
            {
                throw Exceptions.AliyunOssPutObjectError(cause: "Oss上传其他错误", innerException: ex);
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
        public async Task<string> GetFileFromMixedAsync(Directory2 directory, string fileName, bool remoteForced = false)
        {
            string ossKey = GetOssKey(directory, fileName);
            DirectoryDescription description = GetDirectoryDescription(directory);
            TimeSpan localFileExpiryTime = description.ExpiryTime;

            //刚请求过,且存在就返回
            if (!remoteForced && !await _dbLocker.NoWaitLockAsync(nameof(FileManager), ossKey, localFileExpiryTime))
            {
                GlobalSettings.Logger.LogDebug("前不久请求过文件 {osskey}", ossKey);

                //TODO: 思考，这里有一种，第一个线程已经取锁成功，去下载图片，还没下完，第二个进来一看，以为已经取好了，结果没有
                string fullPath = GetLocalFullPath(directory, fileName);

                int tryCount = 3;

                while (tryCount-- > 0)
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        return fullPath;
                    }

                    GlobalSettings.Logger.LogDebug("前不久请求过文件，现在还不存在，等待 1秒，再尝试");
                    await Task.Delay(100);
                }

                GlobalSettings.Logger.LogDebug("前不久请求过文件，现在还不存在，不再等待，远程获取");
            }

            IOss oss = null!;
            try
            {
                oss = await RentOssClientAsync(description.DirectoryPermissionName, false, directory.PlaceHolderValue, remoteForced);

                using OssObject ossObject = oss.GetObject(_options.AliyunOssBucketName, ossKey);

                //覆盖本地
                string localFullPath = await SaveFileToLocalAsync(ossObject.Content, directory, fileName);

                GlobalSettings.Logger.LogDebug("远程文件获取成功，已经保存到本地 {osskey}", ossKey);

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
        /// 返回Null表示失败
        /// </summary>
        public Task<string?> SaveFileToLocalAsync(byte[] data, Directory2 directory, string fileName)
        {
            string fullPath = GetLocalFullPath(directory, fileName);

            return SaveFileToLocalAsync(data, fullPath);
        }

        public async Task<string?> SaveFileToLocalAsync(byte[] data, string fullPath)
        {
            if (await FileUtil.TrySaveFileAsync(data, fullPath))
            {
                return fullPath;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 返回Null表示失败
        /// </summary>
        public Task<string> SaveFileToLocalAsync(Stream stream, Directory2 directory, string fileName)
        {
            string fullPath = GetLocalFullPath(directory, fileName);
            return SaveFileToLocalAsync(stream, fullPath);
        }

        public async Task<string> SaveFileToLocalAsync(Stream stream, string fullPath)
        {
            if (await FileUtil.TrySaveFileAsync(stream, fullPath))
            {
                return fullPath;
            }
            else
            {
                throw ClientExceptions.LocalFileSaveError(fullPath: fullPath);
            }
        }

        private static string GetRandomFileName(string fileExtension)
        {
            //TODO:名字太长，缩短
            return $"r{SecurityUtil.CreateUniqueToken()}{fileExtension}";
        }

        private string GetOssKey(Directory2 directory, string fileName)
        {
            DirectoryDescription description = GetDirectoryDescription(directory);

            string directoryPath = description.GetPath(directory.PlaceHolderValue);

            if(Path.DirectorySeparatorChar == '\\')
            {
                directoryPath = directoryPath.Replace('\\', '/');
            }

            return $"{directoryPath}/{fileName}";
        }

        class OssClientPoolPolicy : IPooledObjectPolicy<IOss>
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
    }
}