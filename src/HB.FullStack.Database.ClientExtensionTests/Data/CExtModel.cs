using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Database.Tests
{
    public class CExtModel : FlackIdModel
    {
        public string? Name { get; set; }
    }

    public static class Mocker
    {
        public static IList<CExtModel> GetCExtModels(int? count = null)
        {
            if (count == null)
            {
                count = 100;
            }

            List<CExtModel> lst = new List<CExtModel>();

            for (int i = 0; i < count; ++i)
            {
                lst.Add(new CExtModel { Name = Guid.NewGuid().ToString() });
            }

            return lst;
        }
    }
}
