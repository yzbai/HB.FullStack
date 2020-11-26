using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Api;
using HB.FullStack.Server;
using Microsoft.AspNetCore.Http;

namespace HB.FullStack.Server.Security
{
    public class DefaultSecurityService : ISecurityService
    {
        private static readonly byte[] _allowedChars = Array.Empty<byte>();

        public DefaultSecurityService()
        {

        }

        //static Random random = new Random();

        public Task<bool> NeedPublicResourceTokenAsync(ApiRequest? apiRequest)
        {
            //TODO:其他安全检测
            //1， 频率. DeviceId, IP. 根据频率来决定客户端要不要弹出防水墙
            //2， 历史登录比较 Mobile和DeviceId绑定。Address

            //检查从某IP，DeviceId，Mobile发来的请求是否需要防水墙。
            //需要的话，查看request.PublicResourceToken. 没有的话，返回ErrorCode.API_NEED_PUBLIC_RESOURCE_TOKEN

            //可以根据不同的ApiRequest类型来判断


            return Task.FromResult(true);

            //if (apiRequest != null && apiRequest.PublicResourceToken.IsNotNullOrEmpty())
            //{
            //    return Task.FromResult(true);
            //}

            //return Task.FromResult(random.Next(0, 10) % 2 == 0);
        }

        public async Task<byte[]> ProcessFormFileAsync(IFormFile? formFile, string[] permittedExtensions, long sizeLimit)
        {
            // Check the file length. This check doesn't catch files that only have 
            // a BOM as their content.
            if (formFile == null || formFile.Length == 0 || permittedExtensions.IsNullOrEmpty())
            {
                throw new ApiException(ErrorCode.ApiUploadEmptyFile);
            }

            if (formFile.Length > sizeLimit)
            {
                throw new ApiException(ErrorCode.ApiUploadOverSize);
            }

            try
            {
                using MemoryStream memoryStream = new MemoryStream();

                await formFile.CopyToAsync(memoryStream).ConfigureAwait(false);

                // Check the content length in case the file's only
                // content was a BOM and the content is actually
                // empty after removing the BOM.
                if (memoryStream.Length == 0)
                {
                    throw new ApiException(ErrorCode.ApiUploadEmptyFile);
                }

                if (!IsValidFileExtensionAndSignature(
                    formFile.FileName, memoryStream, permittedExtensions))
                {
                    throw new ApiException(ErrorCode.ApiUploadWrongType);
                }
                else
                {
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiError, $"文件名称{formFile.FileName}", ex);
            }
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

#pragma warning disable CA1308 // Normalize strings to uppercase
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                if (ext == ".txt" || ext == ".csv" || ext == ".prn")
                {
                    if (_allowedChars.Length == 0)
                    {
                        // Limits characters to ASCII encoding.
                        for (var i = 0; i < data.Length; i++)
                        {
                            if (reader.ReadByte() > sbyte.MaxValue)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // Limits characters to ASCII encoding and
                        // values of the _allowedChars array.
                        for (var i = 0; i < data.Length; i++)
                        {
                            var b = reader.ReadByte();
                            if (b > sbyte.MaxValue ||
                                !_allowedChars.Contains(b))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                // Uncomment the following code block if you must permit
                // files whose signature isn't provided in the _fileSignature
                // dictionary. We recommend that you add file signatures
                // for files (when possible) for all file types you intend
                // to allow on the system and perform the file signature
                // check.
                /*
                if (!_fileSignature.ContainsKey(ext))
                {
                    return true;
                }
                */

                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature.
                List<byte[]>? signatures = FileUtil.FileSignatures[ext];
                byte[]? headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
