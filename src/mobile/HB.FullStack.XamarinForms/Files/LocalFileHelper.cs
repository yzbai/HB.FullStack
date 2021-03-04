using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace HB.FullStack.XamarinForms.Files
{
    public static class LocalFileHelper
    {
        private static Dictionary<string, string> _directoies = new Dictionary<string, string>();

        public static string GetDirectoryPath(string fileCategory, params string?[] subCategories)
        {
            if(!_directoies.ContainsKey(fileCategory))
            {
                _directoies[fileCategory] = Path.Combine(FileSystem.AppDataDirectory, fileCategory.ToString());
            }

            return Path.Combine(_directoies[fileCategory], Path.Combine(subCategories));
        }

        [return:NotNullIfNotNull("fileName")]
        public static string? GetFileFullPath(string? fileName, string fileCategory, params string?[] subCategories)
        {
            if(fileName == null)
            {
                return null;
            }

            fileName = Path.GetFileName(fileName);

            string directory = GetDirectoryPath(fileCategory, subCategories);

            return Path.Combine(directory, fileName);
        }

        public static bool IsFileExisted(string fileName, string fileCategory, params string?[] subCategories)
        {
            string fullPath = GetFileFullPath(fileName, fileCategory, subCategories);
            
            return System.IO.File.Exists(fullPath);
        }

        public static string GetRootDirectory()
        {
            return FileSystem.AppDataDirectory;
        }

        /// <summary>
        /// 返回Null表示失败
        /// </summary>
        public static async Task<string?> SaveFileAsync(byte[] data, string fileName, string fileCategory, params string?[] subCategories)
        {
            string fullPath = GetFileFullPath(fileName, fileCategory, subCategories);

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
        public static async Task<string?> SaveFileAsync(Stream stream, string fileName, string fileCategory, params string?[] subCategories)
        {
            string fullPath = GetFileFullPath(fileName, fileCategory, subCategories);

            if (await FileUtil.TrySaveFileAsync(stream, fullPath).ConfigureAwait(false))
            {
                return fullPath;
            }
            else
            {
                return null;
            }
        }
    }
}
