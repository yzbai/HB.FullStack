using OrmBenchmark.Core;

using ServiceStack.OrmLite;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.OrmLite
{
    public class OrmLiteExecuter : IOrmExecuter
    {
        IDbConnection conn;
        OrmLiteConnectionFactory dbFactory;

        public string Name
        {
            get
            {
                return "Orm Lite";
            }
        }

        public void Init(string connectionStrong)
        {
            dbFactory = new OrmLiteConnectionFactory(connectionStrong, MySqlDialect.Provider);
            conn = dbFactory.Open();
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            object param = new { Id = Id };
            return await conn.SingleAsync<Post>("select * from Posts where Id=@Id", param).ConfigureAwait(false);
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public async Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return await conn.SelectAsync<Post>("select * from Posts").ConfigureAwait(false);
        }

        public IEnumerable<dynamic> GetAllItemsAsDynamic()
        {
            return null;
        }
        public void Finish()
        {
            conn.Close();
        }

    }
}
