using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public AddRequest()
        { }

        public AddRequest(IEnumerable<T> ress) : base(HttpMethodName.Post, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethodName.Post, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(T res) : this(new T[] { res }) { }

        public AddRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

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
    }

    public class AddRequest<T, TParent> : AddRequest<T> where T : ApiResource2 where TParent : ApiResource2
    {
        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public AddRequest()
        { }

        public AddRequest(Guid parentId, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Builder!.AddParent(paretnDef.ResName, parentId.ToString());
        }

        public AddRequest(Guid parentId, T res) : this(parentId, new T[] { res }) { }

        public AddRequest(string apiKeyName, Guid parentId, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Builder!.AddParent(paretnDef.ResName, parentId.ToString());
        }

        public AddRequest(string apiKeyName, Guid parentId, T res) : this(apiKeyName, parentId, new T[] { res }) { }
    }

    public class AddRequest<T, TParent1, TParent2> : AddRequest<T> where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    {
        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public AddRequest()
        { }

        public AddRequest(Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Builder!.AddParent(paretn1Def.ResName, parent1Id.ToString());

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Builder.AddParent(paretn2Def.ResName, parent2Id.ToString());
        }

        public AddRequest(Guid parent1Id, Guid parent2Id, T res) : this(parent1Id, parent2Id, new T[] { res }) { }

        public AddRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Builder!.AddParent(paretn1Def.ResName, parent1Id.ToString());

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Builder.AddParent(paretn2Def.ResName, parent2Id.ToString());
        }

        public AddRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, T res) : this(apiKeyName, parent1Id, parent2Id, new T[] { res }) { }
    }
}