using Dapper;

using MySql.Data.MySqlClient;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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

        public IPost GetItemAsObject(int Id)
        {
            object param = new { Id = Id };
            return conn.Query<Post>("select * from posts where Id=@Id", param, buffered: false).First();
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            object param = new { Id = Id };
            return conn.Query("select * from posts where Id=@Id", param, buffered: false).First();
        }

        public IEnumerable<IPost> GetAllItemsAsObject()
        {
            return conn.Query<Post>("select * from posts", null, buffered: false).ToList<IPost>();
        }

        public IEnumerable<dynamic> GetAllItemsAsDynamic()
        {
            return conn.Query("select * from posts", null, buffered: false).ToList();
        }

        public void Finish()
        {
            conn.Close();
        }
    }
}
