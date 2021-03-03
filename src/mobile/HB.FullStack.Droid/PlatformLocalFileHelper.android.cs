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

[assembly: Dependency(typeof(HB.FullStack.Droid.PlatformLocalFileHelper))]
namespace HB.FullStack.Droid
{
    /// <summary>
    /// 不能放在HB.Framework.Client.Droid中，因为要用到项目中的Resources类
    /// </summary>
    public class PlatformLocalFileHelper : IPlatformLocalFileHelper
    {
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
                using Bitmap? bitmap = await imageSource.GetBitMapAsync().ConfigureAwait(false);

                //using Bitmap scaledBitmap = bitmap.ScaleTo(Avatar_Max_Height, Avatar_Max_Width);

                using FileStream fileStream = new FileStream(fullPath, FileMode.Create);

                if (await bitmap!.CompressAsync(Bitmap.CompressFormat.Png, 100, fileStream).ConfigureAwait(false))
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
    }
}
#nullable restore