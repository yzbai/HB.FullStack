using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Common.Utility
{
    public static class FileHelper
    {
        //public static async Task<byte[]> ReadFormFileAsync(IFormFile file)
        //{
        //    if (file == null)
        //    {
        //        throw new ArgumentNullException(nameof(file));
        //    }

        //    using (Stream stream = file.OpenReadStream())
        //    {
        //        byte[] buffer = new byte[stream.Length];

        //        await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

        //        stream.Close();

        //        return buffer;
        //    }
        //}

        public static void SaveToFile(byte[] buffer, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            using (FileStream fileStream = new FileStream(path, FileMode.CreateNew))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(buffer);

                    binaryWriter.Close();
                }

                fileStream.Close();
            }
        }

        public static byte[] ComputeHash(string filePath)
        {
            var runCount = 1;

            while (runCount < 4)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        using (var fs = File.OpenRead(filePath))
                        {
                            return System.Security.Cryptography.SHA256.Create().ComputeHash(fs);
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
                catch (IOException ex)
                {
                    if (runCount == 3 || ex.HResult != -2147024864)
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

            return new byte[20];
        }
    }
}
