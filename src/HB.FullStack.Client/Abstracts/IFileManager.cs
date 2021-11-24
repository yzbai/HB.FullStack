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
        /// 返回本地 FullPath
        /// </summary>
        Task<string> GetFileAsync(string directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 返回Local FullPath
        /// </summary>
        Task<string> SetFileAsync(string sourceFullPath, string destDirectory, string destFileName, bool recheckPermissionForced = false);

        string GetFullPath(string directory, string fileName);
        string GetNewTempFullPath(string fileExtension);
        Task<string?> SaveFileAsync(byte[] data, string directory, string fileName);
        Task<string?> SaveFileAsync(byte[] data, string fullPath);
        Task<string> SaveFileAsync(Stream stream, string directory, string fileName);
        Task<string> SaveFileAsync(Stream stream, string fullPath);
        Task UnzipAssetZipAsync(string? assetFileName);
    }
}