﻿using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using HB.FullStack.Common;
using HB.FullStack.Common.Files;

namespace HB.FullStack.Client.File
{
    /// <summary>
    /// 模糊本地和远端，只要知道directory和fileName即可
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// 本地和远程同时
        /// 返回本地 FullPath
        /// </summary>
        Task<string> GetFileFromMixedAsync(Directory2 directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 本地和远程同时
        /// 返回Local FullPath
        /// </summary>
        Task<string> SetFileToMixedAsync(string sourceLocalFullPath, Directory2 directory, string fileName, bool recheckPermissionForced = false);

        #region Local

        string GetLocalFullPath(Directory2 directory, string fileName);

        string GetNewTempFullPath(string fileExtension);
        Task<string?> SaveFileToLocalAsync(byte[] data, Directory2 directory, string fileName);
        Task<string?> SaveFileToLocalAsync(byte[] data, string fullPath);
        Task<string> SaveFileToLocalAsync(Stream stream, Directory2 directory, string fileName);
        Task<string> SaveFileToLocalAsync(Stream stream, string fullPath);
        Task UnzipAssetZipAsync(string? assetFileName);

        #endregion
    }
}