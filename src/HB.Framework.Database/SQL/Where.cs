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
    public class Where<T> : SQLExpression
    {
        private IDatabaseEngine _databaseEngine;
        private Expression<Func<T, bool>> _whereExpression;
        private List<string> _orderByProperties;

        private string _whereString;
        private string _orderByString;
        private string _groupByString;
        private string _havingString;
        private string _limitString;

        private long? _limitRows;
        private long? _limitSkip;

        public bool WithWhereString { get; set; }

        public Where(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory modelDefFactory) : base(modelDefFactory)
        {
            _entityDefFactory = modelDefFactory;
            DatabaseEntityDef modelDef = _entityDefFactory.Get<T>();
            _databaseEngine = databaseEngine;
            base.PrefixFieldWithTableName = true;
            base._paramPlaceHolderPrefix = _databaseEngine.ParameterizedChar + "wPARAM__";

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

            _sep = " ";
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
        public virtual Where<T> where(string sqlFilter, params object[] filterParams)
        {
            _whereString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_databaseEngine, sqlFilter, filterParams);

            return this;
        }
        
        public virtual Where<T> where()
        {
            if (_whereExpression != null)
            {
                _whereExpression = null; //Where() clears the expression
            }

            return where(string.Empty);
        }
        
        public virtual Where<T> where(Expression<Func<T, bool>> predicate)
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

        public static string SqlFormat(IDatabaseEngine dbEngine, string sqlText, params object[] sqlParams)
        {
            var escapedParams = new List<string>();

            foreach (var sqlParam in sqlParams)
            {
                if (sqlParam == null)
                {
                    escapedParams.Add("NULL");
                }
                else
                {
                    var sqlInValues = sqlParam as SQLInValues;

                    if (sqlInValues != null)
                    {
                        escapedParams.Add(sqlInValues.ToSqlInString(dbEngine));
                    }
                    else
                    {
                        escapedParams.Add(dbEngine.GetDbValueStatement(sqlParam, needQuoted: true));
                    }
                }
            }
            return string.Format(sqlText, escapedParams.ToArray());
        }

        #endregion

        #region And & Or

        public virtual Where<T> And(Expression<Func<T, bool>> predicate)
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

        public virtual Where<T> Or(Expression<Func<T, bool>> predicate)
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

        public virtual Where<T> GroupBy()
        {
            return GroupBy(string.Empty);
        }

        public virtual Where<T> GroupBy(string groupByString)
        {
            _groupByString = groupByString;
            return this;
        }

        public virtual Where<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sep = string.Empty;
            _groupByString = Visit(keySelector).ToString();

            if (!string.IsNullOrEmpty(_groupByString))
            {
                _groupByString = string.Format("GROUP BY {0}", _groupByString);
            }

            return this;
        }

        #endregion

        #region Having

        public virtual Where<T> Having()
        {
            return Having(string.Empty);
        }

        public virtual Where<T> Having(string sqlFilter, params object[] filterParams)
        {
            _havingString = string.IsNullOrEmpty(sqlFilter) ? string.Empty : SqlFormat(_databaseEngine, sqlFilter, filterParams);

            if (!string.IsNullOrEmpty(_havingString)) 
            {
                _havingString = "HAVING " + _havingString;
            }

            return this;
        }

        public virtual Where<T> Having(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                _sep = " ";
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

        public virtual Where<T> OrderBy()
        {
            return OrderBy(string.Empty);
        }

        public virtual Where<T> OrderBy(string orderBy)
        {
            _orderByProperties.Clear();
            _orderByString = orderBy;
            return this;
        }

        public virtual Where<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sep = string.Empty;
            _orderByProperties.Clear();
            var property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " ASC");
            updateOrderByString();
            return this;
        }

        public virtual Where<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sep = string.Empty;
            var property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " ASC");
            updateOrderByString();
            return this;
        }

        public virtual Where<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sep = string.Empty;
            _orderByProperties.Clear();
            var property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " DESC");
            updateOrderByString();
            return this;
        }

        public virtual Where<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sep = string.Empty;
            var property = Visit(keySelector).ToString();
            _orderByProperties.Add(property + " DESC");
            updateOrderByString();
            return this;
        }

        private void updateOrderByString()
        {
            if (_orderByProperties.Count > 0)
            {
                _orderByString = "ORDER BY ";

                foreach (var prop in _orderByProperties)
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

        public virtual Where<T> Limit(long Skip, long Rows)
        {
            _limitRows = Rows;
            _limitSkip = Skip;

            updateLimitString();

            return this;
        }

        public virtual Where<T> Limit(long Rows)
        {
            _limitRows = Rows;
            _limitSkip = 0;

            updateLimitString();

            return this;
        }

        public virtual Where<T> Limit()
        {
            _limitSkip = null;
            _limitRows = null;

            updateLimitString();

            return this;
        }

        private void updateLimitString()
        {
            if (!_limitSkip.HasValue)
            {
                _limitString = string.Empty;
            }

            string rows = _limitRows.HasValue ? string.Format(",{0}", _limitRows.Value) : string.Empty;

            _limitString = string.Format("LIMIT {0}{1}", _limitSkip.Value, rows);
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

            _sep = " ";
            string newExpr = Visit(predicate).ToString();

            _whereString += string.IsNullOrEmpty(_whereString) ? "" : (" " + appendType + " ");
            _whereString += newExpr;
        }

        public virtual Where<T> And<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public virtual Where<T> And<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("AND", predicate);

            return this;
        }

        public virtual Where<T> Or<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        public virtual Where<T> Or<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> predicate)
        {
            AppendToWhereString("OR", predicate);

            return this;
        }

        #endregion
    }

}


