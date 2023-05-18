using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    public class EmptyApiResource : SharedResource
    {
        public static EmptyApiResource Value { get; }
        public override Guid? Id { get; set; } = null;
        public override long? ExpiredAt { get; set; } = null;

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource()
        { }
    }
}