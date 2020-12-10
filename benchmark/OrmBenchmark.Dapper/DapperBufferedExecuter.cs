using Dapper;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Dapper
{
    public class DapperBufferedExecuter : IOrmExecuter
    {
        SqlConnection conn;

        public string Name
        {
            get
            {
                return "Dapper Query (Buffered)";
            }
        }

        public void Init(string connectionStrong)
        {
            conn = new SqlConnection(connectionStrong);
            conn.Open();
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            object param = new { Id = Id };

            return (await conn.QueryAsync<Post>("select * from Posts where Id=@Id", param/*, buffered: true*/).ConfigureAwait(false)).First();
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public async Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return await conn.QueryAsync<Post>("select * from Posts", null/*, buffered: true*/).ConfigureAwait(false);
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
