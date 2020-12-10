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
    public class OrmLiteNoQueryExecuter : IOrmExecuter
    {
        IDbConnection conn;
        OrmLiteConnectionFactory dbFactory;

        public string Name
        {
            get
            {
                return "Orm Lite (No Query)";
            }
        }

        public void Init(string connectionStrong)
        {
            dbFactory = new OrmLiteConnectionFactory(connectionStrong, MySqlDialect.Provider);
            conn = dbFactory.Open();
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            return await conn.SingleByIdAsync<Post>(Id).ConfigureAwait(false);
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public async Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return await conn.SelectAsync<Post>().ConfigureAwait(false);
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
