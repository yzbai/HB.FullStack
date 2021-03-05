using System.IO;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Base;

using HB.FullStack.XamarinForms;


using Xamarin.Essentials;

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

        Task<string?> GetFileAsync(string directory, string fileName, bool remoteForced = false);

        Task UploadFileAsync(string sourceFullPath, string destDirectory, string destFileName, bool recheckPermissionForced = false);
    }
}