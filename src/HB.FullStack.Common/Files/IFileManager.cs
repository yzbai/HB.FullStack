/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.IO;
using System;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Files
{
    /// <summary>
    /// 模糊本地和远端，只要知道directory和fileName即可.
    /// 模拟本地和远端有着一样的目录结构
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// 本地和远程同时
        /// 返回本地 FullPath
        /// </summary>
        Task<string> GetAsync(Directory2 directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 本地和远程同时
        /// 返回Local FullPath
        /// </summary>
        Task<string> SetAsync(string sourceLocalFullPath, Directory2 directory, string fileName, bool recheckPermissionForced = false);

        Task<string> SetAsync(string sourceLocalFullPath, Directory2 directory, bool recheckPermissionForced = false)
        {
            string newFileName = FileUtil.GetRandomFileName(Path.GetExtension(sourceLocalFullPath));
            return SetAsync(sourceLocalFullPath, directory, newFileName);
        }

        ILocalFileManager LocalFileManager { get; }
    }
}