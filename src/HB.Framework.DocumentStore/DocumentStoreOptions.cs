using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.DocumentStore
{
    public class DocumentStoreSchema
    {
        public string EntityTypeFullName { get; set; }

        public string InstanceName { get; set; }

        public string Database { get; set; }

        public string CollectionName { get; set; }
    }

    public class DocumentStoreOptions : IOptions<DocumentStoreOptions>
    {
        public DocumentStoreOptions Value => this;

        public IList<DocumentStoreSchema> Schemas { get; set; } = new List<DocumentStoreSchema>();

        public DocumentStoreSchema GetDocumentStoreSchema(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Schemas.First(s => type.FullName.Equals(s.EntityTypeFullName, GlobalSettings.Comparison));
        }
    }
}
