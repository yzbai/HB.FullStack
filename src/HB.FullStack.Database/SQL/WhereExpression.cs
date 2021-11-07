#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// SQL����.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereExpression<T>/* : SQLExpression*/
    {
        private readonly SQLExpressionVisitorContenxt _expressionContext;
        private Expression<Func<T, bool>>? _whereExpression;
        private readonly List<string> _orderByProperties = new List<string>();
        private readonly EngineType _engineType;

        private string _whereString = string.Empty;
        private string? _orderByString;
        private string _groupByString = string.Empty;
        private string _havingString = string.Empty;
        private string _limitString = string.Empty;

        private long? _limitRows;
        private long? _limitSkip;

        internal WhereExpression(EngineType engineType)
        {
            _engineType = engineType;

            _expressionContext = new SQLExpressionVisitorContenxt(engineType)
            {
                ParamPlaceHolderPrefix = SqlHelper.PARAMETERIZED_CHAR + "w__"
            };
        }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        /// <summary>
        /// ToStatement
        /// </summary>
        /// <param name="engineType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public string ToStatement(EngineType engineType)
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
        /// ����ַ���ģ��������
        /// </summary>
        /// <param name="sqlFilter">ex: A={0} and B={1} and C in ({2})</param>
        /// <param name="filterParams">ex: ["name",12, new SqlInValues(new int[]{1,2,3})]</param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Where(string sqlFilter, params object[] filterParams)
        {
            _whereString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_engineType, sqlFilter, filterParams);

            return this;
        }

        //public WhereExpression<T> Where()
        //{
        //    if (_whereExpression != null)
        //    {
        //        _whereExpression = null; //Where() clears the expression
        //    }

        //    return Where(string.Empty);
        //}

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

        /// <summary>
        /// ֻ֧�ֲ���TypeConverter(ȫ�ֻ�����������)���ֶ�
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        //TODO:  �����������������һ��sql����ע�����
        /// <exception cref="DatabaseException"></exception>
        private string SqlFormat(EngineType engineType, string sqlText, params object[] sqlParams)
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
                        if (sqlInValues.Count == 0)
                        {
                            escapedParams.Add("NULL");
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();

                            foreach (object value in sqlInValues.Values)
                            {
                                string paramPlaceholder = _expressionContext.GetNextParamPlaceholder();
                                object paramValue = TypeConvert.TypeValueToDbValue(value, null, engineType);
                                
                                _expressionContext.AddParameter(paramPlaceholder, paramValue);

                                sb.Append(paramPlaceholder);
                                sb.Append(',');
                            }

                            sb.Remove(sb.Length - 1, 1);

                            escapedParams.Add(sb.ToString());
                        }
                    }
                    else
                    {
                        string paramPlaceholder = _expressionContext.GetNextParamPlaceholder();
                        object paramValue = TypeConvert.TypeValueToDbValue(sqlParam, null, engineType);

                        _expressionContext.AddParameter(paramPlaceholder, paramValue);
                        escapedParams.Add(paramPlaceholder);
                    }
                }
            }
            return string.Format(GlobalSettings.Culture, sqlText, escapedParams.ToArray());
        }

        #endregion Where

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

        /// <summary>
        /// And
        /// </summary>
        /// <param name="sqlFilter"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> And(string sqlFilter, params object[] filterParams)
        {
            string sql = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_engineType, sqlFilter, filterParams);

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

        /// <summary>
        /// Or
        /// </summary>
        /// <param name="sqlFilter"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Or(string sqlFilter, params object[] filterParams)
        {
            string sql = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_engineType, sqlFilter, filterParams);

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

        #endregion And & Or

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

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            //TODO: �������
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

        #endregion Group By

        #region Having

        /// <summary>
        /// Having
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Having()
        {
            return Having(string.Empty);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="sqlFilter"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Having(string sqlFilter, params object[] filterParams)
        {
            _havingString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_engineType, sqlFilter, filterParams);

            if (!string.IsNullOrEmpty(_havingString))
            {
                _havingString = "HAVING " + _havingString;
            }

            return this;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        #endregion Having

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

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// ThenBy
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// OrderByDescending
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// ThenByDescending
        /// </summary>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
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

        #endregion Order By

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

        #endregion Limit



        #region Multiple

        /// <summary>
        /// AppendToWhereString
        /// </summary>
        /// <param name="appendType"></param>
        /// <param name="predicate"></param>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// And
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> And<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        /// <summary>
        /// And
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> And<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Or<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public WhereExpression<T> Or<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        #endregion Multiple
    }
}