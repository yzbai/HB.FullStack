#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;

using HB.FullStack.XamarinForms.Platforms;

using Java.Util.Zip;

using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(HB.FullStack.Droid.PlatformHelper))]
namespace HB.FullStack.Droid
{
    public class PlatformHelper : IPlatformHelper
    {
        #region StatusBar
        private WindowManagerFlags _orginalFlags;

        public bool IsStatusBarShowing { get; set; } = true;

        public void ShowStatusBar()
        {
            if (IsStatusBarShowing)
            {
                return;
            }

            var attrs = Platform.CurrentActivity.Window!.Attributes;

            attrs!.Flags = _orginalFlags;

            Platform.CurrentActivity.Window.Attributes = attrs;

            IsStatusBarShowing = true;
        }

        public void HideStatusBar()
        {
            if (!IsStatusBarShowing)
            {
                return;
            }

            WindowManagerLayoutParams attrs = Platform.CurrentActivity.Window!.Attributes!;

            _orginalFlags = attrs.Flags;

            attrs.Flags |= WindowManagerFlags.Fullscreen;

            Platform.CurrentActivity.Window.Attributes = attrs;

            IsStatusBarShowing = false;
        }

        #endregion

        #region File

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

        public Stream GetAssetStream(string fileName)
        {
            return Platform.CurrentActivity.Assets!.Open(fileName);
        }

        public async Task<bool> SaveAvatarAsync(ImageSource imageSource, string fullPath)
        {
            try
            {
                using Android.Graphics.Bitmap? bitmap = await imageSource.GetBitMapAsync().ConfigureAwait(false);

                //using Bitmap scaledBitmap = bitmap.ScaleTo(Avatar_Max_Height, Avatar_Max_Width);

                using FileStream fileStream = new FileStream(fullPath, FileMode.Create);

                if (await bitmap!.CompressAsync(Android.Graphics.Bitmap.CompressFormat.Png, 100, fileStream).ConfigureAwait(false))
                {

                    await fileStream.FlushAsync().ConfigureAwait(false);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task UnZipAsync(Stream stream, string directory)
        {
            FileUtil.CreateDirectoryIfNotExist(directory);

            using ZipInputStream zipInputStream = new ZipInputStream(stream);

            ZipEntry? entry = zipInputStream.NextEntry;

            while (entry != null)
            {
                if (!entry.IsDirectory)
                {
                    string path = Path.Combine(directory, entry.Name);
                    await UnZipFileAsync(path, zipInputStream);
                }

                entry = zipInputStream.NextEntry;
            }

            entry?.Dispose();
        }

        private async Task UnZipFileAsync(string fullPath, ZipInputStream zipInputStream)
        {
            FileUtil.CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath));

            using FileStream fileStream = System.IO.File.Open(fullPath, FileMode.Create);
            using BufferedStream bufferedStream = new BufferedStream(fileStream);

            int len;
            byte[] buffer = new byte[4096];

            while ((len = await zipInputStream.ReadAsync(buffer, 0, 4096)) != -1)
            {
                bufferedStream.Write(buffer, 0, len);
            }

            await bufferedStream.FlushAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
#nullable restore