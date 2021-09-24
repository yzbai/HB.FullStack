using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public class FileUpdateRequest<T> : UpdateRequest<T> where T : ApiResource2
    {
        private readonly IEnumerable<byte[]> _files;
        private readonly IEnumerable<string> _fileNames;

        /// <exception cref="ApiException"></exception>
        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resources) : base(resources)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiKeyName"></param>
        /// <param name="files"></param>
        /// <param name="fileNames"></param>
        /// <param name="resouces"></param>
        /// <exception cref="ApiException"></exception>
        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resouces) : base(apiKeyName, resouces)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fileNames"></param>
        /// <param name="resource"></param>
        /// <exception cref="ApiException"></exception>
        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resource) : base(resource)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiKeyName"></param>
        /// <param name="files"></param>
        /// <param name="fileNames"></param>
        /// <param name="resouce"></param>
        /// <exception cref="ApiException"></exception>
        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resouce) : base(apiKeyName, resouce)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        public IEnumerable<byte[]> GetBytess() => _files;

        public string GetBytesPropertyName() => "Files";

        public IEnumerable<string> GetFileNames() => _fileNames;

        /// <summary>
        /// ThrowOnCountNotEven
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fileNames"></param>
        /// <exception cref="ApiException"></exception>
        private static void ThrowOnCountNotEven(IEnumerable<byte[]> files, IEnumerable<string> fileNames)
        {
            if (files.Count() != fileNames.Count())
            {
                throw new ApiException(ApiErrorCodes.ModelValidationError);
            }
        }
    }
}