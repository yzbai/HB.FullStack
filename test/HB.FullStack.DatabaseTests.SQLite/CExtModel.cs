using System;
using System.Collections.Generic;

using HB.FullStack.Database.DBModels;

namespace HB.FullStack.Database.Tests
{
    public class CExtModel : TimelessFlackIdDBModel
    {
        public string? Name { get; set; }
    }

    public static class Mocker
    {
        public static IList<CExtModel> GetCExtModels(int? count = null)
        {
            count ??= 100;

            List<CExtModel> lst = new List<CExtModel>();

            for (int i = 0; i < count; ++i)
            {
                lst.Add(new CExtModel { Name = Guid.NewGuid().ToString() });
            }

            return lst;
        }
    }
}
