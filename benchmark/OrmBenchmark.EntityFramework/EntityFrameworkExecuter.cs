﻿using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrmBenchmark.EntityFramework
{
    public class EntityFrameworkExecuter : IOrmExecuter
    {
        OrmBenchmarkContext ctx;

        public string Name
        {
            get
            {
                return "Entity Framework";
            }
        }

        public void Init(string connectionStrong)
        {

            ctx = new OrmBenchmarkContext(connectionStrong);

        }

        public IPost GetItemAsObjectAsync(int Id)
        {
            return ctx.Posts.Where(p => p.Id == Id) as IPost;

        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return ctx.Posts.Where(p => p.Id == Id).Select(p => new
            {
                p.Id,
                p.Text,
                p.CreationDate,
                p.LastChangeDate,
                p.Counter1,
                p.Counter2,
                p.Counter3,
                p.Counter4,
                p.Counter5,
                p.Counter6,
                p.Counter7,
                p.Counter8,
                p.Counter9,
            });
        }

        public IEnumerable<IPost> GetAllItemsAsObjectAsync()
        {
            return ctx.Posts.ToList<IPost>();
        }

        public IEnumerable<dynamic> GetAllItemsAsDynamic()
        {
            return ctx.Posts.Select(p => new
            {
                p.Id,
                p.Text,
                p.CreationDate,
                p.LastChangeDate,
                p.Counter1,
                p.Counter2,
                p.Counter3,
                p.Counter4,
                p.Counter5,
                p.Counter6,
                p.Counter7,
                p.Counter8,
                p.Counter9,
            }).ToList<dynamic>();
        }
        public void Finish()
        {

        }
    }
}
