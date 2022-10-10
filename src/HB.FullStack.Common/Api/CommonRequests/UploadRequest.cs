using System;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public interface IUploadRequest
    {
        string HttpContentName { get; }

        string FileName { get; }

        byte[] GetFile();
    }

    public sealed class UploadRequest<T> : ApiRequest, IUploadRequest where T : ApiResource
    {
        private readonly byte[]? _file;

        [JsonIgnore]
        public string HttpContentName { get; } = "File";

        [JsonIgnore]
        public string FileName { get; set; } = null!;

        public UploadRequest(byte[] file, string fileName, ApiRequestAuth auth, string? condition) : base(typeof(T).Name, ApiMethod.UpdateProperties, auth, condition)
        {
            _file = file;
            FileName = fileName;
        }

        public byte[] GetFile()
        {
            return _file ?? Array.Empty<byte>();
        }
    }
}