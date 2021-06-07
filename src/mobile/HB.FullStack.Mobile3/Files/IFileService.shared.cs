using System.IO;
using System.Threading.Tasks;
using HB.FullStack.Mobile.Base;

using HB.FullStack.Mobile;


using Xamarin.Essentials;
using Xamarin.Forms;
using System;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Mobile.Files
{
    public interface IFileService
    {

        //TODO: 测试，第一次运行，点击后，立马切换出去，是否正常初始化成功
        /// <summary>
        /// 解压初始文件，常用在下载后的第一次运行
        /// </summary>
        /// <param name="assetFileName"></param>
        /// <returns></returns>
        public static async Task UnzipInitFilesAsync(string? assetFileName)
        {
            //将Assets里的初始文件解压缩到用户文件中去

            if (assetFileName.IsNotNullOrEmpty())
            {
                try
                {
                    using Stream initDatasStream = await FileSystem.OpenAppPackageFileAsync(assetFileName).ConfigureAwait(false);
                    await BaseApplication.PlatformHelper.UnZipAsync(initDatasStream, LocalFileServiceHelper.PathRoot).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    GlobalSettings.Logger.LogCritical(ex, "File Service Unzip Init AssetFile : {assetFileName} Error.", assetFileName);
                }
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