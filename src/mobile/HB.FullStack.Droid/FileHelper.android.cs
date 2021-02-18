#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;


using HB.FullStack.XamarinForms.Platforms;


using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(HB.FullStack.Droid.FileHelper))]
namespace HB.FullStack.Droid
{
    /// <summary>
    /// 不能放在HB.Framework.Client.Droid中，因为要用到项目中的Resources类
    /// </summary>
    public class FileHelper : IFileHelper
    {
        //TODO: 图片存储到公共相册中
        public static readonly string AvatarDirectory = System.IO.Path.Combine(FileSystem.AppDataDirectory, "time_avatars");
        public static readonly string OthersDirectory = System.IO.Path.Combine(FileSystem.AppDataDirectory, "others");

        private readonly object _locker = new object();

        public string GetDirectoryPath(UserFileType fileType)
        {
            return fileType switch
            {
                UserFileType.Avatar => AvatarDirectory,
                _ => OthersDirectory
            };
        }

        public string GetFileSuffix(UserFileType fileType)
        {
            return fileType switch
            {
                UserFileType.Avatar => ".png",
                _ => "",
            };
        }

        public bool IsFileExisted(string fileName, UserFileType userFileType)
        {
            if (!fileName.Contains('.'))
            {
                fileName += GetFileSuffix(userFileType);
            }

            string filePath = System.IO.Path.Combine(GetDirectoryPath(userFileType), fileName);

            return File.Exists(filePath);
        }

        public async Task SaveFileAsync(byte[] data, string fullPath)
        {
            string directory = System.IO.Path.GetDirectoryName(fullPath);

            CreateDirectoryIfNotExist(directory);

            using FileStream fileStream = File.Open(fullPath, FileMode.Create);

            await fileStream.WriteAsync(data).ConfigureAwait(false);

            await fileStream.FlushAsync().ConfigureAwait(false);
        }

        public async Task SaveFileAsync(byte[] data, string fileName, UserFileType userFileType)
        {
            string directory = GetDirectoryPath(userFileType);

            string fullPath = System.IO.Path.Combine(directory, fileName);

            await SaveFileAsync(data, fullPath).ConfigureAwait(false);

            //Make sure it shows up in the Photos gallery promptly.
            if (userFileType == UserFileType.Avatar)
            {
                Android.Media.MediaScannerConnection.ScanFile(Platform.CurrentActivity, new string[] { fullPath }, new string[] { "image/png", "image/jpeg" }, null);
            }
        }

        public async Task<Stream> GetResourceStreamAsync(string resourceName, ResourceType resourceType, string? packageName = null, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = Platform.AppContext.PackageName;
            }
            resourceName = System.IO.Path.GetFileNameWithoutExtension(resourceName);

            int resId = Platform.AppContext.Resources!.GetIdentifier(resourceName, GetResourceTypeName(resourceType), packageName);

            using Stream stream = Platform.CurrentActivity.Resources!.OpenRawResource(resId);

            MemoryStream memoryStream = new MemoryStream();

            if (cancellationToken.HasValue)
            {
                await stream.CopyToAsync(memoryStream, cancellationToken.Value).ConfigureAwait(false);
            }
            else
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            }

            memoryStream.Position = 0;

            return memoryStream;

            static string GetResourceTypeName(ResourceType resourceType)
            {
                return resourceType switch
                {
                    ResourceType.Drawable => "drawable",
                    _ => "",
                };
            }
        }

        public static int GetResourceId2(string resourceName)
        {
            string withoutExtensionFileName = System.IO.Path.GetFileNameWithoutExtension(resourceName);

            //int resId = Platform.CurrentActivity.Resources.GetIdentifier(withoutExtensionFileName, "drawable", "com.brlite.mycolorfultime");
            int resId = (int)typeof(Resource.Drawable).GetField(withoutExtensionFileName).GetValue(null);
            return resId;
        }

        public string GetAssetsHtml(string name)
        {
            AssetManager assetsManager = Platform.CurrentActivity.Assets!;

            using Stream stream = assetsManager.Open(name);

            using StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        private void CreateDirectoryIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                lock (_locker)
                {
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }
            }
        }

        #region Avatar

        public async Task SaveAvatarAsync(ImageSource imageSource, long userId)
        {
            string directoryPath = GetDirectoryPath(UserFileType.Avatar);

            CreateDirectoryIfNotExist(directoryPath);

            string? path = GetAvatarFilePath(userId);

            using Bitmap? bitmap = await imageSource.GetBitMapAsync().ConfigureAwait(false);

            //using Bitmap scaledBitmap = bitmap.ScaleTo(Avatar_Max_Height, Avatar_Max_Width);

            using FileStream fileStream = new FileStream(path, FileMode.Create);

            bool result = await bitmap!.CompressAsync(Bitmap.CompressFormat.Png, 100, fileStream).ConfigureAwait(false);

            await fileStream.FlushAsync().ConfigureAwait(false);
        }

        public string? GetAvatarFilePath(long userId)
        {
            string path = System.IO.Path.Combine(AvatarDirectory, $"{userId}.png");

            return System.IO.File.Exists(path) ? path : null;
        }

        public async Task<byte[]?> GetAvatarAsync(long userId)
        {
            string? filePath = GetAvatarFilePath(userId);

            if (filePath == null)
            {
                return null;
            }

            using FileStream fileStream = new FileStream(filePath, FileMode.Open);

            using MemoryStream memoryStream = new MemoryStream();

            await fileStream.CopyToAsync(memoryStream).ConfigureAwait(false);

            return memoryStream.ToArray();
        }

        #endregion
    }
}
#nullable restore