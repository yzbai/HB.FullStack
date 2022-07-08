using System;
using System.Text.Json.Serialization;
using HB.FullStack.Common.Api.Resources;

namespace HB.FullStack.Common.Api
{
    public interface IUploadRequest
    {
        string HttpContentName { get; }

        string FileName { get; }

        byte[] GetFile();
    }

    /// <summary>
    /// PUT /Ver/ResoruceCollection
    /// </summary>
    public class UploadRequest<T> : UpdateFieldsRequest<T>, IUploadRequest where T : ApiResource
    {
        private readonly byte[]? _file;

        [JsonIgnore]
        public string HttpContentName { get; } = "File";

        [JsonIgnore]
        public string FileName { get; protected set; } = null!;

        [OnlyForJsonConstructor]
        public UploadRequest() { }

        public UploadRequest(byte[] file, string fileName, ApiRequestAuth auth, string? condition) : base(auth, condition)
        {
            _file = file;
            FileName = fileName;
        }

        public byte[] GetFile()
        {
            return _file ?? Array.Empty<byte>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _file?.Length, FileName);
        }
    }
}