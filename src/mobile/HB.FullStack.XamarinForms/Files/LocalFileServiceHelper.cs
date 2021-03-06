using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace HB.FullStack.XamarinForms.Files
{
    /// <summary>
    /// 一个文件 = PathRoot(不同平台不同) + directory + fileName
    /// </summary>
    public static class LocalFileServiceHelper
    {
        public static string PathRoot = FileSystem.AppDataDirectory;

        [return:NotNullIfNotNull("fileName")]
        public static string? GetFullPath(string directory, string fileName)
        {
            return Path.Combine(PathRoot, directory, fileName);
        }

        /// <summary>
        /// 返回Null表示失败
        /// </summary>
        public static Task<string?> SaveFileAsync(byte[] data, string directory, string fileName)
        {
            string fullPath = GetFullPath(directory, fileName);

            return SaveFileAsync(data, fullPath);
        }

        public static async Task<string?> SaveFileAsync(byte[] data, string fullPath)
        {
            if (await FileUtil.TrySaveFileAsync(data, fullPath).ConfigureAwait(false))
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
        public static Task<string> SaveFileAsync(Stream stream, string directory, string fileName)
        {
            string fullPath = GetFullPath(directory, fileName);
            return SaveFileAsync(stream, fullPath);
        }

        private static async Task<string> SaveFileAsync(Stream stream, string fullPath)
        {
            if (await FileUtil.TrySaveFileAsync(stream, fullPath).ConfigureAwait(false))
            {
                return fullPath;
            }
            else
            {
                throw new MobileException(MobileErrorCode.LocalFileSaveError, $"fullPath:{fullPath}");
            }
        }
    }
}
