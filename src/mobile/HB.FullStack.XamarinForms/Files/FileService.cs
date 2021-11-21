using System.IO;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Base;

using HB.FullStack.XamarinForms;

using Xamarin.Essentials;
using Xamarin.Forms;
using System;
using Microsoft.Extensions.Logging;
using HB.FullStack.XamarinForms.Platforms;

namespace HB.FullStack.XamarinForms.Files
{
    public abstract class FileService
    {
        //TODO: 测试，第一次运行，点击后，立马切换出去，是否正常初始化成功
        /// <summary>
        /// 解压初始文件，常用在下载后的第一次运行
        /// </summary>
        public static async Task UnzipInitFilesAsync(string? assetFileName)
        {
            //将Assets里的初始文件解压缩到用户文件中去

            if (assetFileName.IsNotNullOrEmpty())
            {
                try
                {
                    using Stream initDatasStream = await FileSystem.OpenAppPackageFileAsync(assetFileName).ConfigureAwait(false);
                    await PlatformHelper.Current.UnZipAsync(initDatasStream, LocalFileServiceHelper.PathRoot).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    GlobalSettings.Logger.LogCritical(ex, "File Service Unzip Init AssetFile : {assetFileName} Error.", assetFileName);
                }
            }
        }

        /// <summary>
        /// 返回本地 FullPath
        /// </summary>
        public abstract Task<string> GetFileAsync(string directory, string fileName, bool remoteForced = false);

        /// <summary>
        /// 返回Local FullPath
        /// </summary>
        public abstract Task<string> SetFileAsync(string sourceFullPath, string destDirectory, string destFileName, bool recheckPermissionForced = false);

        public abstract ObservableTask<ImageSource> GetImageSourceTask(string directory, string fileName, string? defaultFileName, bool remoteForced = false);

        public abstract ObservableTask<ImageSource> GetImageSourceTask(string directory, string? initFileName, Func<Task<string?>> getFileNameAsyncFunc, string? defaultFileName, bool remoteForced = false);
    }
}