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
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly byte[] _file;
#pragma warning restore CA1051 // Do not declare visible instance fields

        [JsonIgnore]
        public string HttpContentName { get; } = "File";

        [JsonIgnore]
        public string FileName { get; protected set; } = null!;

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

    public class UploadRequest2<T, TOwner> : UploadRequest<T> where T : ApiResource2 where TOwner : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public string OwnerResName { get; set; } = null!;

        public UploadRequest2(Guid ownerId, byte[] file, string fileName, string? condition) : base(file, fileName, condition)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{OwnerResName}/{OwnerId}/{ResName}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
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
            return $"{ApiVersion}/{ResName}/{Id}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id);
        }
    }

    public class UploadByIdRequest2<T, TOwner> : UploadByIdRequest<T> where T : ApiResource2 where TOwner : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public string OwnerResName { get; set; } = null!;

        public UploadByIdRequest2(Guid ownerId, Guid id, byte[] file, string fileName, string? condition) : base(id, file, fileName, condition)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{OwnerResName}/{OwnerId}/{ResName}/{Id}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
        }
    }
}