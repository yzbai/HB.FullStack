﻿

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// SQL语句辅助、工具类
    /// </summary>
    public static class SqlStatement
    {
        #region 表达

        /// <summary>
        /// 请务必在使用前，检查list，不要为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static bool In<T>(T value, bool returnByOrder, IEnumerable list)
        {
            bool do_not_delete_used_in_expression_analysis = returnByOrder;

            if (value == null)
            {
                return false;
            }

            foreach (object? obj in list)
            {
                if (obj == null)
                {
                    continue;
                }

                if (obj.ToString() == value.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// longStr是否包含pattern
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="longStr"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool StartWith<T>(T longStr, T pattern)
        {
            //TODO:  完成SQL like模糊查询的映射
            throw new NotImplementedException();
        }

        public static string Desc<T>(T value)
        {
            return value == null ? "" : value.ToString() + " DESC";
        }

        public static string As<T>(T value, object asValue)
        {
            return value == null ? "" : string.Format(GlobalSettings.Culture, "{0} AS {1}", value.ToString(), asValue);
        }

        public static T Sum<T>(T value)
        {
            return value;
        }

        public static T Count<T>(T value)
        {
            return value;
        }

        public static T Min<T>(T value)
        {
            return value;
        }

        public static T Max<T>(T value)
        {
            return value;
        }

        public static T Avg<T>(T value)
        {
            return value;
        }

        public static T Distinct<T>(T value)
        {
            return value;
        }

        public static T Plain<T>(T value)
        {
            return value;
        }

        #endregion 表达
    }
}