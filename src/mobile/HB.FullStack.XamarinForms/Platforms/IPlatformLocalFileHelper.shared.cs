using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Platforms
{
    public enum ResourceType
    {
        Drawable
    }

    public interface IPlatformLocalFileHelper
    {
        Task<Stream> GetResourceStreamAsync(string fileName, ResourceType resourceType, string? packageName = null, CancellationToken? cancellationToken = null);

        Stream GetAssetStream(string fileName);
    }
}
