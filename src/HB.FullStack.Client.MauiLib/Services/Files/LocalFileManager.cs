using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Client.Services.Files;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Options;
using Microsoft.Maui.Storage;

namespace HB.FullStack.Client.MauiLib.Services.Files
{
    public partial class LocalFileManager : ILocalFileManager
    {
        private readonly Dictionary<string, DirectoryDescription> _directories;
        private readonly FileManagerOptions _options;

        public LocalFileManager(IOptions<FileManagerOptions> options)
        {
            _options = options.Value;
            _directories = _options.DirectoryDescriptions.ToDictionary(d => d.DirectoryName);
        }

        public static string PathRoot { get; } = FileSystem.AppDataDirectory;

        private DirectoryDescription GetDirectoryDescription(Directory2 directory)
        {
            if (_directories.TryGetValue(directory.DirectoryName, out DirectoryDescription? directoryDescription))
            {
                return directoryDescription;
            }

            throw ClientExceptions.NoSuchDirectory(directory.DirectoryName);
        }

        [return: NotNullIfNotNull(nameof(fileName))]
        public string? GetFullPath(Directory2 directory, string fileName)
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

        /// <summary>
        /// 返回Null表示失败
        /// </summary>
        public Task<string?> SaveFileAsync(byte[] data, Directory2 directory, string fileName)
        {
            string fullPath = GetFullPath(directory, fileName);

            return SaveFileAsync(data, fullPath);
        }

        public async Task<string?> SaveFileAsync(byte[] data, string fullPath)
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
        public Task<string> SaveFileAsync(Stream stream, Directory2 directory, string fileName)
        {
            string fullPath = GetFullPath(directory, fileName);
            return SaveFileAsync(stream, fullPath);
        }

        public async Task<string> SaveFileAsync(Stream stream, string fullPath)
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
    }
}