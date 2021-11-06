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
        public string FileName { get; private set; } = null!;

        public UploadRequest(byte[] file, string fileName, string? condition) : base(condition)
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

   

    public class UploadRequest<T, TSub> : UpdateFieldsRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        private readonly byte[] _file;

        [JsonIgnore]
        public string HttpContentName { get; } = "File";

        [JsonIgnore]
        public string FileName { get; private set; } = null!;

        public UploadRequest(Guid id, byte[] file, string fileName, string? condition) : base(id, condition)
        {
            _file = file;
            FileName = fileName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _file.Length, FileName);
        }
    }

    public class UploadByIdRequest<T> : UploadRequest<T> where T : ApiResource2
    {
        [JsonIgnore, NoEmptyGuid]
        public Guid Id { get; private set; }

        public UploadByIdRequest(Guid id, byte[] file, string fileName, string? condition) : base(file, fileName, condition)
        {
            Id = id;
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{ResourceCollectionName}/{Id}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id);
        }
    }

    public class UploadByIdRequest<T, TSub> : UploadRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        [JsonIgnore, NoEmptyGuid]
        public Guid SubId { get; private set; }

        public UploadByIdRequest(Guid id, Guid subId, byte[] file, string fileName, string? condition) : base(id, file, fileName, condition)
        {
            SubId = subId;
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{ResourceCollectionName}/{Id}/{SubResourceCollectionName}/{SubId}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SubId);
        }
    }
}