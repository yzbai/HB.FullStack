using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        public DeleteRequest(IEnumerable<T> ress) : base(HttpMethodName.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethodName.Delete, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res) : this(new T[] { res }) { }

        public DeleteRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

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

    public class DeleteRequest<T, TParent> : DeleteRequest<T> where T : ApiResource2 where TParent : ApiResource2
    {
        public DeleteRequest(Guid parentId, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Builder!.Parents.Add((paretnDef.ResName, parentId.ToString()));
        }

        public DeleteRequest(Guid parentId, T res) : this(parentId, new T[] { res }) { }

        public DeleteRequest(string apiKeyName, Guid parentId, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Builder!.Parents.Add((paretnDef.ResName, parentId.ToString()));
        }

        public DeleteRequest(string apiKeyName, Guid parentId, T res) : this(apiKeyName, parentId, new T[] { res }) { }
    }

    public class DeleteRequest<T, TParent1, TParent2> : DeleteRequest<T> where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    {
        public DeleteRequest(Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Builder!.Parents.Add((paretn1Def.ResName, parent1Id.ToString()));

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Builder!.Parents.Add((paretn2Def.ResName, parent2Id.ToString()));
        }

        public DeleteRequest(Guid parent1Id, Guid parent2Id, T res) : this(parent1Id, parent2Id, new T[] { res }) { }

        public DeleteRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Builder!.Parents.Add((paretn1Def.ResName, parent1Id.ToString()));

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Builder!.Parents.Add((paretn2Def.ResName, parent2Id.ToString()));
        }

        public DeleteRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, T res) : this(apiKeyName, parent1Id, parent2Id, new T[] { res }) { }
    }
}