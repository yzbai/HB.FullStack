using System.IO;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Base;

using HB.FullStack.XamarinForms;


using Xamarin.Essentials;
using Xamarin.Forms;
using System;

namespace HB.FullStack.XamarinForms.Files
{
    public interface IFileService
    {
        public static async Task InitializeAsync(string assetFileName)
        {
            if (VersionTracking.IsFirstLaunchEver)
            {
                //将Assets里的初始文件解压缩到用户文件中去

                using Stream initDatasStream = await FileSystem.OpenAppPackageFileAsync(assetFileName).ConfigureAwait(false);
                await BaseApplication.PlatformHelper.UnZipAsync(initDatasStream, LocalFileServiceHelper.PathRoot).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 返回本地 FullPath
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="remoteForced"></param>
        /// <returns></returns>
        Task<string> GetFileAsync(string directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 返回Local FullPath
        /// </summary>
        /// <param name="sourceFullPath"></param>
        /// <param name="destDirectory"></param>
        /// <param name="destFileName"></param>
        /// <param name="recheckPermissionForced"></param>
        /// <returns></returns>
        Task<string> SetFileAsync(string sourceFullPath, string destDirectory, string destFileName, bool recheckPermissionForced = false);

        ObservableTask<ImageSource> GetImageSourceTask(string directory, string fileName, string? defaultFileName, bool remoteForced = false);
        ObservableTask<ImageSource> GetImageSourceTask(string directory, string? initFileName, Func<Task<string?>> getFileNameAsyncFunc, string? defaultFileName, bool remoteForced = false);
    }
}