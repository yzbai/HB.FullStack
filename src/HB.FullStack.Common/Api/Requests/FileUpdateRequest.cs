using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class FileUpdateRequest<T> : UpdateRequest<T> where T : Resource
    {
        private readonly IEnumerable<byte[]> _files;
        private readonly IEnumerable<string> _fileNames;

        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resources) : base(resources)
        {
            if (files.Count() != fileNames.Count())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, System.Net.HttpStatusCode.BadRequest);
            }

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resouces) : base(apiKeyName, resouces)
        {
            if (files.Count() != fileNames.Count())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, System.Net.HttpStatusCode.BadRequest);
            }

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resource) : base(resource)
        {
            if (files.Count() != fileNames.Count())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, System.Net.HttpStatusCode.BadRequest);
            }

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resouce) : base(apiKeyName, resouce)
        {
            if (files.Count() != fileNames.Count())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, System.Net.HttpStatusCode.BadRequest);
            }

            _files = files;
            _fileNames = fileNames;
        }

#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<byte[]> GetBytess() => _files;

        public string GetBytesPropertyName() => "Files";

        public IEnumerable<string> GetFileNames() => _fileNames;
#pragma warning restore CA1024 // Use properties where appropriate
    }
}