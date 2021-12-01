using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

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
    public class UploadRequest<T> : UpdateFieldsRequest<T>, IUploadRequest where T : ApiResource2
    {
        private readonly byte[] _file;

        [JsonIgnore]
        public string HttpContentName { get; } = "File";

        [JsonIgnore]
        public string FileName { get; protected set; } = null!;

        public UploadRequest(byte[] file, string fileName, string? condition, Guid? ownerResId, Guid? resId) : base(resId, ownerResId, condition)
        {
            _file = file;
            FileName = fileName;
        }

        public byte[] GetFile()
        {
            return _file;
        }

        public override string ToDebugInfo()
        {
            return $"FileUpdateRequest. FileNames:{SerializeUtil.ToJson(FileName)}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _file.Length, FileName);
        }
    }
}