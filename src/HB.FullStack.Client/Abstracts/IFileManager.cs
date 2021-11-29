using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using HB.FullStack.Common;

namespace HB.FullStack.Client
{
    public  interface IFileManager
    {
        /// <summary>
        /// 本地和远程同时
        /// 返回本地 FullPath
        /// </summary>
        Task<string> GetFileAsync(string directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 本地和远程同时
        /// 返回Local FullPath
        /// </summary>
        Task<string> SetFileAsync(string sourceFullPath, string destDirectory, string destFileName, bool recheckPermissionForced = false);

        string GetLocalFullPath(string directory, string fileName);
        string GetNewTempFullPath(string fileExtension);
        Task<string?> SaveFileToLocalAsync(byte[] data, string directory, string fileName);
        Task<string?> SaveFileToLocalAsync(byte[] data, string fullPath);
        Task<string> SaveFileToLocalAsync(Stream stream, string directory, string fileName);
        Task<string> SaveFileToLocalAsync(Stream stream, string fullPath);
        Task UnzipAssetZipAsync(string? assetFileName);
    }
}