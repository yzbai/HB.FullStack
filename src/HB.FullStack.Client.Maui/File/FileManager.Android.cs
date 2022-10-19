using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Java.Util.Zip;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace HB.FullStack.Client.Maui.File
{
    public partial class FileManager
    {
        //TODO: 测试，第一次运行，点击后，立马切换出去，是否正常初始化成功
        public async Task UnzipAssetZipAsync(string? assetFileName)
        {
            //将Assets里的初始文件解压缩到用户文件中去

            if (assetFileName.IsNotNullOrEmpty())
            {
                try
                {
                    using Stream initDatasStream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
                    await UnZipAsync(initDatasStream, PathRoot);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    GlobalSettings.Logger?.LogCritical(ex, "File Service Unzip Init AssetFile : {AssetFileName} Error.", assetFileName);
                }
            }
        }

        public static async Task UnZipAsync(Stream stream, string directory)
        {
            FileUtil.CreateDirectoryIfNotExist(directory);

            using ZipInputStream zipInputStream = new ZipInputStream(stream);

            ZipEntry? entry = zipInputStream.NextEntry;

            while (entry != null)
            {
                if (!entry.IsDirectory)
                {
                    string path = Path.Combine(directory, entry.Name!);
                    await UnZipFileAsync(path, zipInputStream);
                }

                entry = zipInputStream.NextEntry;
            }
        }

        private static async Task UnZipFileAsync(string fullPath, ZipInputStream zipInputStream)
        {
            FileUtil.CreateDirectoryIfNotExist(Path.GetDirectoryName(fullPath)!);

            using FileStream fileStream = System.IO.File.Open(fullPath, FileMode.Create);
            using BufferedStream bufferedStream = new BufferedStream(fileStream);

            int len;
            byte[] buffer = new byte[4096];

            while ((len = await zipInputStream.ReadAsync(buffer, 0, 4096)) != -1)
            {
                await bufferedStream.WriteAsync(buffer.AsMemory(0, len));
            }

            await bufferedStream.FlushAsync();
        }
    }
}
