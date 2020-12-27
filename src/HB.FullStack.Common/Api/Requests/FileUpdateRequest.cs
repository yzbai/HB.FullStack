using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class FileUpdateRequest<T> : ApiRequest<T> where T : Resource
    {
        private readonly byte[]? _file;

        private readonly string _fileName;

        public FileUpdateRequest(byte[] file, string fileName) : base(HttpMethod.Put, null)
        {
            _file = file;
            _fileName = fileName;
        }

        public FileUpdateRequest(string apiKeyName, byte[] file, string fileName) : base(apiKeyName, HttpMethod.Put, null)
        {
            _file = file;
            _fileName = fileName;
        }

        [IdBarrier]
        [Required]
        public T Resource { get; set; } = null!;

        public byte[]? GetBytes() => _file;

        public string GetBytesPropertyName() => "File";

        public string GetFileName() => _fileName;

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }
}