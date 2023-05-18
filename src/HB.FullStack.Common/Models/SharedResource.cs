using System;

namespace HB.FullStack.Common.Models
{
    /// <summary>
    /// One kind of Data Transfer Objects.Mainly using on net.
    /// </summary>
    public abstract class SharedResource : ValidatableObject, IModel, IExpired
    {
        public ModelKind GetKind()
        {
            return ModelKind.Shared;
        }

        public abstract Guid? Id { get; set; }

        public abstract long? ExpiredAt { get; set; }
    }
}