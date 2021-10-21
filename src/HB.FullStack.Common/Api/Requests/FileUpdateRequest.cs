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

        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resources) : base(resources)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resouces) : base(apiKeyName, resouces)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resource) : base(resource)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resouce) : base(apiKeyName, resouce)
        {
            ThrowOnCountNotEven(files, fileNames);

            _files = files;
            _fileNames = fileNames;
        }

        public IEnumerable<byte[]> GetBytess() => _files;

        public string GetBytesPropertyName() => "Files";

        public IEnumerable<string> GetFileNames() => _fileNames;

        private static void ThrowOnCountNotEven(IEnumerable<byte[]> files, IEnumerable<string> fileNames)
        {
            if (files.Count() != fileNames.Count())
            {
                throw Exceptions.FileUpdateRequestCountNotEven();
            }
        }
    }
}