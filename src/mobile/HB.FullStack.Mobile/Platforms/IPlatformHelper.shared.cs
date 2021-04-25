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

    public interface IPlatformHelper
    {
        #region StatusBar

        bool IsStatusBarShowing { get; }

        void ShowStatusBar();

        void HideStatusBar();

        #endregion

        #region File

        Task<Stream> GetResourceStreamAsync(string fileName, ResourceType resourceType, string? packageName = null, CancellationToken? cancellationToken = null);
        Task UnZipAsync(Stream stream, string directory);

        #endregion

        //Stream GetAssetStream(string fileName);
    }
}
