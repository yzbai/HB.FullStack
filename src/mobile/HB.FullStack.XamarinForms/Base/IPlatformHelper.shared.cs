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
        private static IPlatformHelper? _current;
        public static IPlatformHelper Current
        {
            get
            {
                if (_current == null)
                {
                    _current = DependencyService.Resolve<IPlatformHelper>();
                }

                return _current;
            }
        }

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
