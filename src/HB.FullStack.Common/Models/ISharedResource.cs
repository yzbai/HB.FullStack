using System;

namespace HB.FullStack.Common.Models
{
    /// <summary>
    /// One kind of Data Transfer Objects.Mainly using on net.
    /// </summary>

    public interface ISharedResource : IModel, IExpired
    {
        //object? Id { get; set; }
    }

    public abstract class SharedResource2<TId> : ValidatableObject, ISharedResource
    {
        public ModelKind GetKind()
        {
            return ModelKind.Shared;
        }

        public abstract TId? Id { get; set; }

        public abstract long? ExpiredAt { get; set; }

        //object? ISharedResource.Id { get => Id; set => Id = (TId?)value; }
    }
}