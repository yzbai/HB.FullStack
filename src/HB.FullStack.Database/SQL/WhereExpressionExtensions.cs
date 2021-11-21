using HB.FullStack.Database.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Database.SQL
{
    public static class WhereExpressionExtensions
    {
        public static WhereExpression<T> AddOrderAndLimits<T>(this WhereExpression<T> where, int? page, int? perPage, string? orderBy) where T : DatabaseEntity, new()
        {
            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (orderBy.IsNotNullOrEmpty())
            {
                string[] orderNames = orderBy.Trim().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder orderBuilder = new StringBuilder();

                foreach (string orderName in orderNames)
                {
                    if (!entityDef.ContainsProperty(orderName))
                    {
                        throw DatabaseExceptions.NoSuchProperty(entityDef.EntityFullName, orderName);
                    }

                    orderBuilder.Append(SqlHelper.GetQuoted(orderName));
                    orderBuilder.Append(',');
                }

                orderBuilder.RemoveLast();

                where.OrderBy(orderBuilder.ToString());
            }

            if (page.HasValue && perPage.HasValue)
            {
                where.Limit(page.Value * perPage.Value, perPage.Value);
            }

            return where;
        }
    }
}