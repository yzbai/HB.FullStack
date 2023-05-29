using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    public class EmptyApiResource : ValidatableObject, ISharedResource
    {
        public static EmptyApiResource Value { get; }
        public object? Id { get; set; }
        public long? ExpiredAt { get; set; }

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource()
        { }

        public ModelKind GetKind() => ModelKind.Shared;
    }
}