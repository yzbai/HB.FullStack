using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class FileUpdateRequest<T> : UpdateRequest<T> where T : ApiResource2
    {
        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resources) : base(resources)
        {
            ThrowOnCountNotEven(files, fileNames);

            Files = files;
            FileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> resouces) : base(apiKeyName, resouces)
        {
            ThrowOnCountNotEven(files, fileNames);

            Files = files;
            FileNames = fileNames;
        }

        public FileUpdateRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resource) : base(resource)
        {
            ThrowOnCountNotEven(files, fileNames);

            Files = files;
            FileNames = fileNames;
        }

        public FileUpdateRequest(string apiKeyName, IEnumerable<byte[]> files, IEnumerable<string> fileNames, T resouce) : base(apiKeyName, resouce)
        {
            ThrowOnCountNotEven(files, fileNames);

            Files = files;
            FileNames = fileNames;
        }

        [JsonIgnore]
        public IEnumerable<byte[]> Files { get; private set; }

        [JsonIgnore]
        public string BytesPropertyName { get; } = "Files";

        [JsonIgnore]
        public IEnumerable<string> FileNames { get; private set; } = null!;

        private static void ThrowOnCountNotEven(IEnumerable<byte[]> files, IEnumerable<string> fileNames)
        {
            if (files.Count() != fileNames.Count())
            {
                throw ApiExceptions.FileUpdateRequestCountNotEven();
            }
        }
    }
}