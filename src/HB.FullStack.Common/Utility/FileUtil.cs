#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace System
{
    public static class FileUtil
    {
        public static IDictionary<string, List<byte[]>> FileSignatures { get; private set; } = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        public static bool IsFileExtensionMatched(string? fileName, string[] allowedExtensions)
        {
            if (fileName.IsNullOrEmpty() || allowedExtensions.IsNullOrEmpty())
            {
                return false;
            }

#pragma warning disable CA1308 // Normalize strings to uppercase
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            if (fileExtension.IsNullOrEmpty() || !allowedExtensions.Contains(fileExtension))
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> IsFileSignatureMatchedAsync(string extension, Stream? stream)
        {
            if (extension.IsNullOrEmpty() || stream == null)
            {
                return false;
            }

            if (!FileSignatures.ContainsKey(extension))
            {
                return false;
            }

            try
            {
                using MemoryStream memoryStream = new MemoryStream();

                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

                if (memoryStream.Length == 0)
                {
                    return false;
                }

                using BinaryReader binaryReader = new BinaryReader(memoryStream);

                List<byte[]> signatures = FileSignatures[extension];
                byte[]? headerBytes = binaryReader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger?.LogError(ex, $"IsFileSignatureMatched Error.extension: {extension}");
                return false;
            }
        }

        public static bool TrySaveToFile(byte[] buffer, string path)
        {
            try
            {
                using FileStream fileStream = new FileStream(path, FileMode.CreateNew);
                using BinaryWriter binaryWriter = new BinaryWriter(fileStream);

                binaryWriter.Write(buffer);

                binaryWriter.Close();

                fileStream.Close();

                return true;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (System.ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// ComputeFileHash
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>




        public static byte[] ComputeFileHash(string filePath)
        {
            int runCount = 1;

            while (runCount < 4)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        using FileStream fs = File.OpenRead(filePath);
                        using System.Security.Cryptography.SHA256 sha256Obj = System.Security.Cryptography.SHA256.Create();
                        return sha256Obj.ComputeHash(fs);
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
                catch (IOException ex)
                {
                    //-2147024864意思是 另一个程序正在使用此文件,进程无法访问
#pragma warning disable CA1508 // Avoid dead conditional code
                    if (runCount == 3 || ex.HResult != -2147024864)
#pragma warning restore CA1508 // Avoid dead conditional code
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, runCount)));
                        runCount++;
                    }
                }
            }

            throw new FileLoadException();
        }
    }
}