using HB.FullStack.XamarinForms.Platforms;

using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms
{
    public enum ResourceType
    {
        Drawable
    }

    public abstract class PlatformHelper
    {
        private static PlatformHelper? _current;

        public static PlatformHelper Current
        {
            get
            {
                if (_current == null)
                {
                    _current = DependencyService.Resolve<PlatformHelper>();
                }

                return _current;
            }
        }

        #region StatusBar

        public bool IsStatusBarShowing { get; protected set; }

        public abstract void ShowStatusBar();

        public abstract void HideStatusBar();

        #endregion

        #region File

        public abstract Task<Stream> GetResourceStreamAsync(string fileName, ResourceType resourceType, string? packageName = null, CancellationToken? cancellationToken = null);

        public abstract Task UnZipAsync(Stream stream, string directory);

        public abstract Task<bool> SaveImageSourceAsync(ImageSource imageSource, string fullPath);

        public abstract Stream GetAssetStream(string assetFileName);

        #endregion

        //Stream GetAssetStream(string fileName);
    }
}