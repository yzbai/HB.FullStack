using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Common;

using Xamarin.Forms;


namespace HB.FullStack.XamarinForms
{
    public class ImageSourceManager
    {
        private readonly IFileManager _fileManager;

        public ImageSourceManager(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public ObservableTask<ImageSource> GetImageSourceTask(string directory, string fileName, string? defaultFileName, bool remoteForced = false)
        {
            string localFullPath = _fileManager.GetFullPath(directory, fileName);

            ImageSource initImageSource;

            if (File.Exists(localFullPath))
            {
                initImageSource = ImageSource.FromFile(localFullPath);
            }
            else
            {
                initImageSource = ImageSource.FromFile(defaultFileName);
            }

            return new ObservableTask<ImageSource>(
                initImageSource,
                async () =>
                {
                    string? fullPath = await _fileManager.GetFileAsync(directory, fileName, remoteForced).ConfigureAwait(false);

                    return fullPath.IsNullOrEmpty() ? initImageSource : ImageSource.FromFile(fullPath);
                });
        }

        public ObservableTask<ImageSource> GetImageSourceTask(string directory, string? initFileName, Func<Task<string?>> getFileNameAsyncFunc, string? defaultFileName, bool remoteForced = false)
        {
            ImageSource? initImageSource = null;

            if (initFileName.IsNotNullOrEmpty())
            {
                string localFullPath = _fileManager.GetFullPath(directory, initFileName);

                if (File.Exists(localFullPath))
                {
                    initImageSource = ImageSource.FromFile(localFullPath);
                }
            }

            if (initImageSource == null)
            {
                initImageSource = ImageSource.FromFile(defaultFileName);
            }

            return new ObservableTask<ImageSource>(
                initImageSource,
                async () =>
                {
                    string? fileName = await getFileNameAsyncFunc().ConfigureAwait(false);

                    if (fileName.IsNullOrEmpty())
                    {
                        return initImageSource;
                    }

                    string? fullPath = await _fileManager.GetFileAsync(directory, fileName, remoteForced).ConfigureAwait(false);

                    return fullPath.IsNullOrEmpty() ? initImageSource : ImageSource.FromFile(fullPath);
                });
        }
    }
}