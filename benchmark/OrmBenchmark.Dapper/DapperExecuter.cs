using Dapper;

using MySql.Data.MySqlClient;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Dapper
{
    public class DapperExecuter : IOrmExecuter
    {
        MySqlConnection conn;

        public string Name
        {
            get
            {
                return "Dapper Query (Non Buffered)";
            }
        }

        public void Init(string connectionStrong)
        {
            conn = new MySqlConnection(connectionStrong);
            conn.Open();
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            object param = new { Id = Id };
            return (await conn.QueryAsync<Post>("select * from posts where Id=@Id", param/*, buffered: false*/).ConfigureAwait(false)).First();
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public   Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return null;
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
