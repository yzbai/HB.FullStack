using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
{
    /// <summary>
    /// SQL条件.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereExpression<T> : SQLExpression
    {
        private readonly IDatabaseEngine _databaseEngine;
        private Expression<Func<T, bool>> _whereExpression;
        private readonly List<string> _orderByProperties;

        private string _whereString;
        private string _orderByString;
        private string _groupByString;
        private string _havingString;
        private string _limitString;

        private long? _limitRows;
        private long? _limitSkip;

        public bool WithWhereString { get; set; }

        public WhereExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory modelDefFactory) : base(modelDefFactory)
        {
            EntityDefFactory = modelDefFactory;
            //DatabaseEntityDef modelDef = EntityDefFactory.GetDef<T>();
            _databaseEngine = databaseEngine;
            PrefixFieldWithTableName = true;
            ParamPlaceHolderPrefix = _databaseEngine.ParameterizedChar + "wPARAM__";

            _whereExpression = null;
            _whereString = string.Empty;
            WithWhereString = true;
            
            _orderByProperties = new List<string>();
            _orderByString = string.Empty;

            _groupByString = string.Empty;
            _havingString = string.Empty;    
            _limitString = string.Empty;
        }

        protected override IDatabaseEngine GetDatabaseEngine()
        {
            return _databaseEngine;
        }

        public override string ToString()
        {
            StringBuilder sql = new StringBuilder();

            Seperator = " ";
            string lamdaWhereString = Visit(_whereExpression).ToString();

            bool hasLamdaWhere = !string.IsNullOrEmpty(lamdaWhereString);
            bool hasStringWhere = !string.IsNullOrEmpty(_whereString);

            if (WithWhereString && (hasLamdaWhere || hasStringWhere))
            {
                sql.Append(" WHERE ");
            }

            if (hasLamdaWhere)
            {
                sql.Append("( ");
                sql.Append(lamdaWhereString);
                sql.Append(" ) ");
            }

            if (hasStringWhere)
            {
                if (hasLamdaWhere)
                {
                    sql.Append(" AND ");
                }

                sql.Append("( ");
                sql.Append(_whereString);
                sql.Append(" ) ");
            }

            sql.Append(string.IsNullOrEmpty(_groupByString) ?
                       "" :
                       "\n" + _groupByString);

            sql.Append(string.IsNullOrEmpty(_havingString) ?
                       "" :
                       "\n" + _havingString);

            sql.Append(string.IsNullOrEmpty(_orderByString) ?
                       "" :
                       "\n" + _orderByString);

            sql.Append(string.IsNullOrEmpty(_limitString) ?
                        "" :
                        "\n" + _limitString);

            return sql.ToString();
        }

        #region Where

        /// <summary>
        /// 添加字符串模板条件。
        /// </summary>
        /// <param name="sqlFilter">ex: A={0} and B={1} and C in ({2})</param>
        /// <param name="filterParams">ex: ["name",12, new SqlInValues(new int[]{1,2,3})]</param>
        /// <returns></returns>
        public virtual WhereExpression<T> Where(string sqlFilter, params object[] filterParams)
        {
            _whereString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_databaseEngine, sqlFilter, filterParams);

            return this;
        }
        
        public virtual WhereExpression<T> Where()
        {
            if (_whereExpression != null)
            {
                _whereExpression = null; //Where() clears the expression
            }

            return Where(string.Empty);
        }

        public virtual WhereExpression<T> Where(Expression<Func<T, bool>> predicate)
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

        public string SqlFormat(IDatabaseEngine dbEngine, string sqlText, params object[] sqlParams)
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
                        escapedParams.Add(sqlInValues.ToSqlInString(dbEngine));
                    }
                    else
                    {
                        escapedParams.Add(dbEngine.GetDbValueStatement(sqlParam, needQuoted: true));
                    }
                }
            }
            return string.Format(GlobalSettings.Culture, sqlText, escapedParams.ToArray());
        }

        #endregion

        #region And & Or

        public virtual WhereExpression<T> And(Expression<Func<T, bool>> predicate)
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

        public virtual WhereExpression<T> Or(Expression<Func<T, bool>> predicate)
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

        #endregion

        #region Group By

        public virtual WhereExpression<T> GroupBy()
        {
            return GroupBy(string.Empty);
        }

        public virtual WhereExpression<T> GroupBy(string groupByString)
        {
            _groupByString = groupByString;
            return this;
        }

        public virtual WhereExpression<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Seperator = string.Empty;
            _groupByString = Visit(keySelector).ToString();

            if (!string.IsNullOrEmpty(_groupByString))
            {
                _groupByString = string.Format(GlobalSettings.Culture, "GROUP BY {0}", _groupByString);
            }

            return this;
        }

        #endregion

        #region Having

        public virtual WhereExpression<T> Having()
        {
            return Having(string.Empty);
        }

        public virtual WhereExpression<T> Having(string sqlFilter, params object[] filterParams)
        {
            _havingString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_databaseEngine, sqlFilter, filterParams);

            if (!string.IsNullOrEmpty(_havingString)) 
            {
                _havingString = "HAVING " + _havingString;
            }

            return this;
        }

        public virtual WhereExpression<T> Having(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                Seperator = " ";
                _havingString = Visit(predicate).ToString();

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

        public virtual WhereExpression<T> OrderBy()
        {
            return OrderBy(string.Empty);
        }

        public virtual WhereExpression<T> OrderBy(string orderBy)
        {
            _orderByProperties.Clear();
            _orderByString = orderBy;
            return this;
        }

        public virtual WhereExpression<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Seperator = string.Empty;
            _orderByProperties.Clear();
            string property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " ASC");
            UpdateOrderByString();
            return this;
        }

        public virtual WhereExpression<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Seperator = string.Empty;
            string property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " ASC");
            UpdateOrderByString();
            return this;
        }

        public virtual WhereExpression<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Seperator = string.Empty;
            _orderByProperties.Clear();
            string property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " DESC");
            UpdateOrderByString();
            return this;
        }

        public virtual WhereExpression<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Seperator = string.Empty;
            string property = Visit(keySelector).ToString();
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

        public virtual WhereExpression<T> Limit(long Skip, long Rows)
        {
            _limitRows = Rows;
            _limitSkip = Skip;

            UpdateLimitString();

            return this;
        }

        public virtual WhereExpression<T> Limit(long Rows)
        {
            _limitRows = Rows;
            _limitSkip = 0;

            UpdateLimitString();

            return this;
        }

        public virtual WhereExpression<T> Limit()
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

            _limitString = string.Format(GlobalSettings.Culture, "LIMIT {0}{1}", _limitSkip.Value, rows);
        }

        #endregion 

        #region Multiple

        protected void AppendToWhereString(string appendType, Expression predicate)
        {
            PrefixFieldWithTableName = true;

            if (predicate == null)
            {
                return;
            }

            Seperator = " ";
            string newExpr = Visit(predicate).ToString();

            _whereString += string.IsNullOrEmpty(_whereString) ? "" : (" " + appendType + " ");
            _whereString += newExpr;
        }

        public virtual WhereExpression<T> And<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public virtual WhereExpression<T> And<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public virtual WhereExpression<T> Or<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        public virtual WhereExpression<T> Or<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        #endregion
    }

}


