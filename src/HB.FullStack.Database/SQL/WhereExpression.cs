#nullable enable

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// SQL条件.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereExpression<T>/* : SQLExpression*/
    {
        private readonly SQLExpressionVisitorContenxt _expressionContext;
        private Expression<Func<T, bool>>? _whereExpression;
        private readonly List<string> _orderByProperties = new List<string>();

        private string _whereString = string.Empty;
        private string? _orderByString;
        private string _groupByString = string.Empty;
        private string _havingString = string.Empty;
        private string _limitString = string.Empty;

        private long? _limitRows;
        private long? _limitSkip;

        internal WhereExpression(IDatabaseEntityDefFactory entityDefFactory)
        {
            _expressionContext = new SQLExpressionVisitorContenxt(entityDefFactory)
            {
                ParamPlaceHolderPrefix = SqlHelper.ParameterizedChar + "w__"
            };
        }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public string ToString(DatabaseEngineType engineType)
        {
            StringBuilder sql = new StringBuilder();

            bool hasStringWhere = !string.IsNullOrEmpty(_whereString);
            bool hasLamdaWhere = _whereExpression != null;

            if (hasStringWhere || hasLamdaWhere)
            {
                sql.Append(" WHERE ");
            }

            if (hasStringWhere)
            {
                sql.Append($" ({_whereString}) ");
            }

            if (hasLamdaWhere)
            {
                if (hasStringWhere)
                {
                    sql.Append(" AND ");
                }

                sql.Append($" ({_whereExpression!.ToStatement(_expressionContext)}) ");
            }

            sql.Append($" {_groupByString} {_havingString} ");

            if (!_orderByString.IsNullOrEmpty())
            {
                sql.Append(_orderByString);
            }
            else if (!_expressionContext.OrderByStatementBySQLUtilIn_QuotedColName.IsNullOrEmpty())
            {
                sql.Append(SqlHelper.GetOrderBySqlUtilInStatement(
                    _expressionContext.OrderByStatementBySQLUtilIn_QuotedColName!,
                    _expressionContext.OrderByStatementBySQLUtilIn_Ins!,
                    engineType
                    ));
            }

            sql.Append($" {_limitString} ");

            return sql.ToString();
        }

        #region Where

        /// <summary>
        /// 添加字符串模板条件。
        /// </summary>
        /// <param name="sqlFilter">ex: A={0} and B={1} and C in ({2})</param>
        /// <param name="filterParams">ex: ["name",12, new SqlInValues(new int[]{1,2,3})]</param>
        /// <returns></returns>
        public WhereExpression<T> Where(string sqlFilter, params object[] filterParams)
        {
            _whereString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : WhereExpression<T>.SqlFormat(sqlFilter, filterParams);

            return this;
        }

        public WhereExpression<T> Where()
        {
            if (_whereExpression != null)
            {
                _whereExpression = null; //Where() clears the expression
            }

            return Where(string.Empty);
        }

        public WhereExpression<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                And(predicate);
            }
            else
            {
                _whereExpression = null;
            }

            return this;
        }

        //TODO:  参数化，而不是组成一个sql，有注入风险
        private static string SqlFormat(string sqlText, params object[] sqlParams)
        {
            List<string> escapedParams = new List<string>();

            foreach (object sqlParam in sqlParams)
            {
                if (sqlParam == null)
                {
                    escapedParams.Add("NULL");
                }
                else
                {

                    if (sqlParam is SQLInValues sqlInValues)
                    {
                        escapedParams.Add(sqlInValues.ToSqlInString());
                    }
                    else
                    {
                        escapedParams.Add(TypeConverter.TypeValueToDbValueStatement(sqlParam, quotedIfNeed: true));
                    }
                }
            }
            return string.Format(GlobalSettings.Culture, sqlText, escapedParams.ToArray());
        }

        #endregion

        #region And & Or

        public WhereExpression<T> And(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                if (_whereExpression == null)
                {
                    _whereExpression = predicate;
                }
                else
                {
                    _whereExpression = _whereExpression.And(predicate);
                }
            }
            return this;
        }

        public WhereExpression<T> And(string sqlFilter, params object[] filterParams)
        {
            string sql = string.IsNullOrEmpty(sqlFilter) ? string.Empty : WhereExpression<T>.SqlFormat(sqlFilter, filterParams);

            if (_whereString.IsNullOrEmpty())
            {
                _whereString = sql;
            }
            else
            {
                _whereString = $" ({_whereString}) AND ({sql})";
            }

            return this;
        }

        public WhereExpression<T> Or(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                if (_whereExpression == null)
                {
                    _whereExpression = predicate;
                }
                else
                {
                    _whereExpression = _whereExpression.Or(predicate);
                }
            }
            return this;
        }

        public WhereExpression<T> Or(string sqlFilter, params object[] filterParams)
        {
            string sql = string.IsNullOrEmpty(sqlFilter) ? string.Empty : WhereExpression<T>.SqlFormat(sqlFilter, filterParams);

            if (_whereString.IsNullOrEmpty())
            {
                _whereString = sql;
            }
            else
            {
                _whereString = $" ({_whereString}) OR ({sql})";
            }

            return this;
        }

        #endregion

        #region Group By

        public WhereExpression<T> GroupBy()
        {
            return GroupBy(string.Empty);
        }

        public WhereExpression<T> GroupBy(string groupByString)
        {
            _groupByString = groupByString;
            return this;
        }

        public WhereExpression<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            //TODO: 调查这个
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;
            _groupByString = keySelector.ToStatement(_expressionContext);
            _expressionContext.Seperator = oldSeparator;

            if (!string.IsNullOrEmpty(_groupByString))
            {
                _groupByString = string.Format(GlobalSettings.Culture, "GROUP BY {0}", _groupByString);
            }

            return this;
        }

        #endregion

        #region Having

        public WhereExpression<T> Having()
        {
            return Having(string.Empty);
        }

        public WhereExpression<T> Having(string sqlFilter, params object[] filterParams)
        {
            _havingString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : WhereExpression<T>.SqlFormat(sqlFilter, filterParams);

            if (!string.IsNullOrEmpty(_havingString))
            {
                _havingString = "HAVING " + _havingString;
            }

            return this;
        }

        public WhereExpression<T> Having(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                string oldSeparator = _expressionContext.Seperator;
                _expressionContext.Seperator = " ";
                _havingString = predicate.ToStatement(_expressionContext);
                _expressionContext.Seperator = oldSeparator;

                if (!string.IsNullOrEmpty(_havingString))
                {
                    _havingString = "HAVING " + _havingString;
                }
            }
            else
            {
                _havingString = string.Empty;
            }

            return this;
        }

        #endregion

        #region Order By

        public WhereExpression<T> OrderBy()
        {
            return OrderBy(string.Empty);
        }

        public WhereExpression<T> OrderBy(string orderBy)
        {
            _orderByProperties.Clear();
            _orderByString = orderBy;
            return this;
        }

        public WhereExpression<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;

            _orderByProperties.Clear();

            string property = keySelector.ToStatement(_expressionContext);

            _expressionContext.Seperator = oldSeparator;

            _orderByProperties.Add(property + " ASC");

            UpdateOrderByString();

            return this;
        }

        public WhereExpression<T> OrderBy<TTarget, TKey>(Expression<Func<TTarget, TKey>> keySelector)
        {
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;

            _orderByProperties.Clear();

            string property = keySelector.ToStatement(_expressionContext);

            _expressionContext.Seperator = oldSeparator;

            _orderByProperties.Add(property + " ASC");

            UpdateOrderByString();

            return this;
        }


        public WhereExpression<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;

            string property = keySelector.ToStatement(_expressionContext);

            _expressionContext.Seperator = oldSeparator;

            _orderByProperties.Add(property + " ASC");

            UpdateOrderByString();

            return this;
        }

        public WhereExpression<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;

            _orderByProperties.Clear();
            string property = keySelector.ToStatement(_expressionContext);

            _expressionContext.Seperator = oldSeparator;

            _orderByProperties.Add(property + " DESC");
            UpdateOrderByString();
            return this;
        }

        public WhereExpression<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            string oldSeparator = _expressionContext.Seperator;
            _expressionContext.Seperator = string.Empty;

            string property = keySelector.ToStatement(_expressionContext);

            _expressionContext.Seperator = oldSeparator;

            _orderByProperties.Add(property + " DESC");
            UpdateOrderByString();
            return this;
        }

        private void UpdateOrderByString()
        {
            if (_orderByProperties.Count > 0)
            {
                _orderByString = "ORDER BY ";

                foreach (string prop in _orderByProperties)
                {
                    _orderByString += prop + ",";
                }

                _orderByString = _orderByString.TrimEnd(',');
            }
            else
            {
                _orderByString = null;
            }
        }

        #endregion

        #region Limit

        public WhereExpression<T> Limit(long Skip, long Rows)
        {
            _limitRows = Rows;
            _limitSkip = Skip;

            UpdateLimitString();

            return this;
        }

        public WhereExpression<T> Limit(long Rows)
        {
            _limitRows = Rows;
            _limitSkip = 0;

            UpdateLimitString();

            return this;
        }

        public WhereExpression<T> Limit()
        {
            _limitSkip = null;
            _limitRows = null;

            UpdateLimitString();

            return this;
        }

        private void UpdateLimitString()
        {
            if (!_limitSkip.HasValue)
            {
                _limitString = string.Empty;
            }

            string rows = _limitRows.HasValue ? string.Format(GlobalSettings.Culture, ",{0}", _limitRows.Value) : string.Empty;

            _limitString = string.Format(GlobalSettings.Culture, "LIMIT {0}{1}", _limitSkip!.Value, rows);
        }

        #endregion 

        #region Multiple

        protected void AppendToWhereString(string appendType, Expression predicate)
        {
            if (predicate == null)
            {
                return;
            }

            string oldSeperator = _expressionContext.Seperator;
            _expressionContext.Seperator = " ";
            string newExpr = predicate.ToStatement(_expressionContext);
            _expressionContext.Seperator = oldSeperator;

            _whereString += string.IsNullOrEmpty(_whereString) ? "" : (" " + appendType + " ");
            _whereString += newExpr;
        }

        public WhereExpression<T> And<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public WhereExpression<T> And<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public WhereExpression<T> Or<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        public WhereExpression<T> Or<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        #endregion
    }

}


