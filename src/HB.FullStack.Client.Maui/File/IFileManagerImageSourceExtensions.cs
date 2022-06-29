using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Client.File;
using HB.FullStack.Common;
using HB.FullStack.Common.Files;

using Microsoft.Maui.Controls;

namespace System
{
    public static class IFileManagerImageSourceExtensions
    {
        public static ObservableTask<ImageSource> GetImageSource(this IFileManager fileManager, Directory2 directory, string? fileName, string defaultFileName, bool remoteForced = false)
        {
            ImageSource? initImageSource = null;

            if (fileName.IsNotNullOrEmpty())
            {
                string localFullPath = fileManager.GetLocalFullPath(directory, fileName);

                if (System.IO.File.Exists(localFullPath))
                {
                    initImageSource = ImageSource.FromFile(localFullPath);
                }
            }
            else
            {
                return new ObservableTask<ImageSource>(ImageSource.FromFile(defaultFileName), null);
            }

            if (initImageSource == null)
            {
                initImageSource = ImageSource.FromFile(defaultFileName);
            }

            return new ObservableTask<ImageSource>(
                initImageSource,
                async () =>
                {
                    string? fullPath = await fileManager.GetFileFromMixedAsync(directory, fileName, remoteForced);

                    return fullPath.IsNullOrEmpty() ? initImageSource : ImageSource.FromFile(fullPath);
                });
        }

        public static ObservableTask<ImageSource> GetImageSource(this IFileManager fileManager,
            Directory2 directory, 
            string? initFileName,
            Func<Task<string?>> updateFileNameAsyncFunc, 
            string defaultFileName, 
            bool remoteForced = false)
        {
            ImageSource? initImageSource = null;

            if (initFileName.IsNotNullOrEmpty())
            {
                string localFullPath = fileManager.GetLocalFullPath(directory, initFileName);

                if (System.IO.File.Exists(localFullPath))
                {
                    initImageSource = ImageSource.FromFile(localFullPath);
                }
            }

            if (initImageSource == null)
            {
                initImageSource = ImageSource.FromFile(defaultFileName);
            }

            if (updateFileNameAsyncFunc == null)
            {
                return new ObservableTask<ImageSource>(initImageSource, null);
            }

            return new ObservableTask<ImageSource>(
                initImageSource,
                async () =>
                {
                    string? fileName = await updateFileNameAsyncFunc();

                    if (fileName.IsNullOrEmpty())
                    {
                        return initImageSource;
                    }

                    string? fullPath = await fileManager.GetFileFromMixedAsync(directory, fileName, remoteForced);

                    return fullPath.IsNullOrEmpty() ? initImageSource : ImageSource.FromFile(fullPath);
                });
        }
    }
}