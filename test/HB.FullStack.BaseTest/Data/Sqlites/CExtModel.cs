using System;
using System.Collections.Generic;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{
    [DbModel(DbSchema_Sqlite)]
    public class CExtModel : TimelessFlackIdDbModel
    {
        public string? Name { get; set; }
    }

    public static partial class Mocker
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
