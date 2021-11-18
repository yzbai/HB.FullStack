using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.Tests
{
    public class CExtEntity : FlackIdEntity
    {
        public string? Name { get; set; }
    }

    public static class Mocker
    {
        public static IList<CExtEntity> GetCExtEntities(int? count = null)
        {
            if (count == null)
            {
                count = 100;
            }

            List<CExtEntity> lst = new List<CExtEntity>();

            for (int i = 0; i < count; ++i)
            {
                lst.Add(new CExtEntity { Name = Guid.NewGuid().ToString() });
            }

            return lst;
        }
    }
}
