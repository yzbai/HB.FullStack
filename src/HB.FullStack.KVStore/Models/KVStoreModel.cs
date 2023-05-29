using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public interface IKVStoreModel : IModel, ITimestamp
    {
        string SubstituteKey { get; set; }
        string? LastUser { get; set; }
    }

    public class KVStoreModel : ValidatableObject, IKVStoreModel
    {
        [Required]
        [KVStoreSubstituteKey]
        [CacheModelKey]
        public string SubstituteKey { get; set; } = SecurityUtil.CreateUniqueToken();

        public string? LastUser { get; set; }

        public long Timestamp { get; set; } = -1;

        public ModelKind GetKind() => ModelKind.KV;

    }
}