using HB.Framework.Common;
using HB.Framework.Common.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DocumentStore
{
    public class DocumentStoreEntity : CommonEntity
    {
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}
