﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public UpdateRequest() { }

        public UpdateRequest(IEnumerable<T> ress, ApiRequestAuth auth, string? condition) : base(HttpMethodName.Put, auth, condition)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res, ApiRequestAuth auth, string? condition) : this(new T[] { res },auth, condition) { }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(base.GetHashCode());

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }

        protected sealed override ApiRequestBuilder CreateBuilder()
        {
            return new RestfulApiRequestBuilder<T>(this);
        }
    }
}