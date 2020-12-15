using Dapper.Contrib.Extensions;

using MySqlConnector;

//using MySql.Data.MySqlClient;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Dapper
{
    public class DapperContribExecuter : IOrmExecuter
    {
        MySqlConnection conn;

        string _connectionString;

        public string Name
        {
            get
            {
                return "Dapper Contrib";
            }
        }

        public void Init(string connectionStrong)
        {
            //conn = new MySqlConnection(connectionStrong);
            _connectionString = connectionStrong;
            //conn.Open();
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            MySqlConnection mySqlConnection = new MySqlConnection(_connectionString);
            var rt = await mySqlConnection.GetAsync<Post>(Id).ConfigureAwait(false);
            //conn.Close();
            return rt;
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public async Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return null;
        }

        public IEnumerable<dynamic> GetAllItemsAsDynamic()
        {
            return null;
        }

        public void Finish()
        {
            //conn.Close();
        }
    }
}
