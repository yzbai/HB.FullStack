using Dapper.Contrib.Extensions;

using MySql.Data.MySqlClient;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace OrmBenchmark.Dapper
{
    public class DapperContribExecuter : IOrmExecuter
    {
        MySqlConnection conn;

        public string Name
        {
            get
            {
                return "Dapper Contrib";
            }
        }

        public void Init(string connectionStrong)
        {
            conn = new MySqlConnection(connectionStrong);
            conn.Open();
        }

        public IPost GetItemAsObject(int Id)
        {
            return conn.Get<Post>(Id);
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;
        }

        public IEnumerable<IPost> GetAllItemsAsObject()
        {
            return conn.GetAll<Post>().ToList<IPost>();
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
