using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// PUT /Ver/ResoruceCollection
    /// </summary>
    public class UploadRequest : ApiRequest
    {
        [JsonIgnore]
        public IEnumerable<byte[]> Files { get; private set; }

        [JsonIgnore]
        public string BytesPropertyName { get; } = "Files";

        [JsonIgnore]
        public IEnumerable<string> FileNames { get; private set; } = null!;

        public UploadRequest(
            string? endPointName,
            string? apiVersion,
            string? resourceName,
            string? resourceCollectionName,
            string? condition,
            TimeSpan? rateLimit, 
            IEnumerable<byte[]> files, 
            IEnumerable<string> fileNames)
            : base(HttpMethod.Put, ApiAuthType.Jwt, endPointName, apiVersion, resourceName, resourceCollectionName, condition, rateLimit)
        {
            ThrowOnCountNotEven(files, fileNames);

            Files = files;
            FileNames = fileNames;
        }

        public override string ToDebugInfo()
        {
            return $"FileUpdateRequest. FileNames:{SerializeUtil.ToJson(FileNames)}";
        }

        private static void ThrowOnCountNotEven(IEnumerable<byte[]> files, IEnumerable<string> fileNames)
        {
            if (files.Count() != fileNames.Count())
            {
                throw ApiExceptions.FileUpdateRequestCountNotEven();
            }
        }

        protected override HashCode GetChildHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(typeof(UploadRequest).FullName);

            foreach (string fileName in FileNames)
            {
                hashCode.Add(fileName);
            }

            return hashCode;
        }
    }


    public class UploadRequest<T> : UploadRequest where T : ApiResource2
    {
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        public UploadRequest(IEnumerable<byte[]> files, IEnumerable<string> fileNames, IEnumerable<T> ress)
            : base(null, null, null, null, null, null, files, fileNames)
        {
            Resources.AddRange(ress);

            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResourceName = def.ResourceName;
            ResourceCollectionName = def.ResourceCollectionName;
            RateLimit = def.RateLimit;
        }

        protected override HashCode GetChildHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(typeof(UploadRequest<T>).FullName);

            hash.Add(base.GetChildHashCode());

            return hash;
        }
    }
}