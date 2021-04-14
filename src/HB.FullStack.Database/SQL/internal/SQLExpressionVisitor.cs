#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.SQL
{
    internal static class SQLExpressionVisitor
    {
        /// <summary>
        /// Visit
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static object Visit(Expression? exp, SQLExpressionVisitorContenxt context)
        {
            if (exp == null) return string.Empty;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp, context);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp, context);

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp, context);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp, context);

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);

                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp, context);

                case ExpressionType.New:
                    return VisitNew((NewExpression)exp, context);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp, context);

                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);

                default:
                    return exp.ToString();
            }
        }

        /// <summary>
        /// VisitLambda
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitLambda(LambdaExpression lambda, SQLExpressionVisitorContenxt context)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && context.Seperator == " ")
            {
                MemberExpression m = (MemberExpression)lambda.Body;

                if (m.Expression != null)
                {
                    object r = VisitMemberAccess(m, context);

                    if (!(r is PartialSqlString))
                    {
                        return r;
                    }

                    if (!m.Expression.Type.IsValueType)
                    {
                        return r.ToString()!;
                    }

                    //return $"{r}={context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)}";
                    return $"{r}=1";
                }
            }
            return Visit(lambda.Body, context);
        }

        /// <summary>
        /// VisitBinary
        /// </summary>
        /// <param name="b"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitBinary(BinaryExpression b, SQLExpressionVisitorContenxt context)
        {
            object left;
            object right;
            bool rightIsNull;
            string operand = BindOperant(b.NodeType);

            if (operand == "AND" || operand == "OR")
            {
                if (b.Left is MemberExpression m && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    //left = new PartialSqlString(string.Format(GlobalSettings.Culture, "{0}={1}", VisitMemberAccess(m, context), context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)));
                    left = new PartialSqlString($"{VisitMemberAccess(m, context)}=1");
                }
                else
                {
                    left = Visit(b.Left, context);
                }

                if (b.Right is MemberExpression mm && mm.Expression != null && mm.Expression.NodeType == ExpressionType.Parameter)
                {
                    right = new PartialSqlString($"{VisitMemberAccess(mm, context)}=1");
                }
                else
                {
                    right = Visit(b.Right, context);
                }

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    object result = Expression.Lambda(b).Compile().DynamicInvoke()!;
                    return new PartialSqlString(TypeConvert.TypeValueToDbValueStatement(result, quotedIfNeed: true, context.EngineType));
                }

                if (left as PartialSqlString == null)
                {
                    left = ((bool)left) ? _trueExpression : _falseExpression;
                }

                if (right as PartialSqlString == null)
                {
                    right = ((bool)right) ? _trueExpression : _falseExpression;
                }
            }
            else
            {
                left = Visit(b.Left, context);
                right = Visit(b.Right, context);

                if(left is EnumMemberAccess enumMemberAccess)
                {
                    //将right转为enum对应的字符串
                    right = Enum.ToObject(enumMemberAccess.EnumType, right).ToString();
                }

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    object result = Expression.Lambda(b).Compile().DynamicInvoke()!;
                    return result;
                }
                else if (left as PartialSqlString == null)
                {
                    left = TypeConvert.TypeValueToDbValueStatement(left, quotedIfNeed: true, context.EngineType);
                }
                else if (right as PartialSqlString == null)
                {
                    if (!context.IsParameterized /*|| right == null*/)
                    {
                        right = TypeConvert.TypeValueToDbValueStatement(right, quotedIfNeed: true, context.EngineType);
                    }
                    else
                    {
                        string paramPlaceholder = context.ParamPlaceHolderPrefix + context.ParamCounter++;
                        context.AddParameter(paramPlaceholder, right);
                        right = paramPlaceholder;
                    }
                }
            }

            //TODO: Test  switch InvariantCultureIgnoreCase to OrdinalIgnoreCase
            rightIsNull = right.ToString()!.Equals("null", GlobalSettings.ComparisonIgnoreCase);

            if (operand == "=" && rightIsNull)
            {
                operand = "is";
            }
            else if (operand == "<>" && rightIsNull)
            {
                operand = "is not";
            }

            return operand switch
            {
                "MOD" => new PartialSqlString(string.Format(GlobalSettings.Culture, "{0}({1},{2})", operand, left, right)),
                "COALESCE" => new PartialSqlString(string.Format(GlobalSettings.Culture, "{0}({1},{2})", operand, left, right)),
                _ => new PartialSqlString("(" + left + context.Seperator + operand + context.Seperator + right + ")")
            };
        }

        /// <summary>
        /// VisitMemberAccess
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitMemberAccess(MemberExpression m, SQLExpressionVisitorContenxt context)
        {
            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
            {
                string memberName = m.Member.Name;
                Type entityType = m.Expression.Type;

                if (m.Expression.NodeType == ExpressionType.Convert)
                {
                    object obj = Visit(m.Expression, context);

                    if (obj is Type type)
                    {
                        entityType = type;
                    }
                    else
                    {
                        //TODO: Test this;
                        return obj.GetType().GetProperty(memberName)!.GetValue(obj)!;
                        //return obj.GetType().InvokeMember(memberName, System.Reflection.BindingFlags.GetProperty, null, obj, null);
                    }
                }

                EntityDef entityDef = EntityDefFactory.GetDef(entityType)!;
                EntityPropertyDef propertyDef = entityDef.GetPropertyDef(m.Member.Name)
                    ?? throw Exceptions.EntityError(entityDef.EntityFullName, m.Member.Name, "Lack property definition");

                string prefix = "";

                if (context.PrefixFieldWithTableName && !string.IsNullOrEmpty(entityDef.DbTableReservedName))
                {
                    prefix = entityDef.DbTableReservedName + ".";
                }

                if (propertyDef.Type.IsEnum)
                    return new EnumMemberAccess(prefix + propertyDef.DbReservedName, propertyDef.Type);

                return new PartialSqlString(prefix + propertyDef.DbReservedName);
            }

            var member = Expression.Convert(m, typeof(object));
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        private static object VisitMemberInit(MemberInitExpression exp/*, SQLExpressionVisitorContenxt context*/)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke()!;
        }

        /// <summary>
        /// VisitNew
        /// </summary>
        /// <param name="nex"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitNew(NewExpression nex, SQLExpressionVisitorContenxt context)
        {
            // TODO : check !
            var member = Expression.Convert(nex, typeof(object));
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                return getter();
            }
            catch (InvalidOperationException)
            { // FieldName ?
                List<object> exprs = VisitExpressionList(nex.Arguments, context);
                StringBuilder r = new StringBuilder();
                foreach (object e in exprs)
                {
                    r.AppendFormat(GlobalSettings.Culture, "{0}{1}",
                                   r.Length > 0 ? "," : "",
                                   e);
                }
                return r.ToString();
            }
        }

        private static object VisitParameter(ParameterExpression p/*, SQLExpressionVisitorContenxt context*/)
        {
            //return p.Name;
            return p.Type;
        }

        private static object VisitConstant(ConstantExpression c/*, SQLExpressionVisitorContenxt context*/)
        {
            if (c.Value == null)
            {
                return new PartialSqlString("null");
            }

            return c.Value;

            //if (!IsParameterized)
            //{
            //    return c.Value;
            //}
            //else
            //{
            //    string paramPlaceholder = db.ParamString + "CONS_" + paramCounter++;
            //    //Params.Add()
            //    //Params.Add(paramPlaceholder, c.Value);
            //    return new PartialSqlString(paramPlaceholder);
            //}
        }

        /// <summary>
        /// VisitUnary
        /// </summary>
        /// <param name="u"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitUnary(UnaryExpression u, SQLExpressionVisitorContenxt context)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    object o = Visit(u.Operand, context);

                    if (o as PartialSqlString == null)
                        return !((bool)o);

                    if (IsTableField(u.Type, o))
                        o += "=1";

                    return new PartialSqlString("NOT (" + o + ")");

                case ExpressionType.Convert:
                    if (u.Method != null)
                        return Expression.Lambda(u).Compile().DynamicInvoke()!;
                    break;
            }

            return Visit(u.Operand, context);
        }

        /// <summary>
        /// VisitMethodCall
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            if (m.Method.DeclaringType == typeof(SqlStatement))
                return VisitSqlMethodCall(m, context);

            if (IsArrayMethod(m))
                return VisitArrayMethodCall(m, context);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m, context);

            return Expression.Lambda(m).Compile().DynamicInvoke()!;
        }

        /// <summary>
        /// VisitExpressionList
        /// </summary>
        /// <param name="original"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static List<object> VisitExpressionList(ReadOnlyCollection<Expression> original, SQLExpressionVisitorContenxt context)
        {
            List<object> list = new List<object>();

            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit || original[i].NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList((NewArrayExpression)original[i], context));
                }
                else
                {
                    list.Add(Visit(original[i], context));
                }
            }
            return list;
        }

        /// <summary>
        /// VisitNewArray
        /// </summary>
        /// <param name="na"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitNewArray(NewArrayExpression na, SQLExpressionVisitorContenxt context)
        {
            List<object> exprs = VisitExpressionList(na.Expressions, context);
            StringBuilder r = new StringBuilder();
            foreach (object e in exprs)
            {
                r.Append(r.Length > 0 ? "," + e : e);
            }

            return r.ToString();
        }

        /// <summary>
        /// VisitNewArrayFromExpressionList
        /// </summary>
        /// <param name="na"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static List<object> VisitNewArrayFromExpressionList(NewArrayExpression na, SQLExpressionVisitorContenxt context)
        {
            List<object> exprs = VisitExpressionList(na.Expressions, context);
            return exprs;
        }

        /// <summary>
        /// VisitArrayMethodCall
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitArrayMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = VisitExpressionList(m.Arguments, context);
                    object quotedColName = args[1];

                    Expression memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression)!;

                    var member = Expression.Convert(memberExpr, typeof(object));
                    Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    object[] inArgs = (object[])getter();

                    StringBuilder sIn = new StringBuilder();
                    foreach (object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            sIn.AppendFormat(GlobalSettings.Culture, "{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         TypeConvert.TypeValueToDbValueStatement(e, quotedIfNeed: true, context.EngineType));
                        }
                        else
                        {
                            IList<object> listArgs = (IList<object>)e;

                            foreach (object el in listArgs)
                            {
                                sIn.AppendFormat(GlobalSettings.Culture, "{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         TypeConvert.TypeValueToDbValueStatement(el, quotedIfNeed: true, context.EngineType));
                            }
                        }
                    }

                    statement = string.Format(GlobalSettings.Culture, "{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        /// <summary>
        /// VisitSqlMethodCall
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitSqlMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            List<object> args = VisitExpressionList(m.Arguments, context);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":
                    UnaryExpression member = Expression.Convert(m.Arguments[2], typeof(object));
                    Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
                    Func<object> getter = lambda.Compile();

                    IEnumerable inArgs = (IEnumerable)getter();

                    List<string> sIn = new List<string>();

                    foreach (object e in inArgs)
                    {
                        if (!typeof(ICollection).GetTypeInfo().IsAssignableFrom(e.GetType()))
                        {
                            sIn.Add(TypeConvert.TypeValueToDbValueStatement(e, quotedIfNeed: true, context.EngineType));
                        }
                        else
                        {
                            ICollection listArgs = (ICollection)e;
                            foreach (object el in listArgs)
                            {
                                sIn.Add(TypeConvert.TypeValueToDbValueStatement(el, quotedIfNeed: true, context.EngineType));
                            }
                        }
                    }

                    if (sIn.Count == 0)
                    {
                        //防止集合为空
                        sIn.Add("null");
                    }

                    string joinedSIn = sIn.ToJoinedString(",");

                    statement = string.Format(GlobalSettings.Culture, "{0} {1} ({2})", quotedColName, m.Method.Name, joinedSIn);

                    if (Convert.ToBoolean(args[0], GlobalSettings.Culture))
                    {
                        context.OrderByStatementBySQLUtilIn_Ins = sIn.ToArray();
                        context.OrderByStatementBySQLUtilIn_QuotedColName = quotedColName.ToString();
                    }

                    break;

                case "Desc":
                    statement = string.Format(GlobalSettings.Culture, "{0} DESC", quotedColName);
                    break;

                case "As":
                    statement = string.Format(GlobalSettings.Culture, "{0} As {1}", quotedColName,
                        SqlHelper.GetQuoted(RemoveQuoteFromAlias(args[0].ToString()!)));
                    break;

                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                case "Distinct":
                    statement = string.Format(GlobalSettings.Culture, "{0}({1}{2})",
                                         m.Method.Name,
                                         quotedColName,
                                         args.Count == 1 ? string.Format(GlobalSettings.Culture, ",{0}", args[0]) : "");
                    break;

                case "Plain":
                    statement = quotedColName.ToString()!;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        /// <summary>
        /// VisitColumnAccessMethod
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static object VisitColumnAccessMethod(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            #region Mysql,其他数据库可能需要重写

            //TODO: Mysql,其他数据库可能需要重写
            if (m.Method.Name == "StartsWith")
            {
                List<object> args0 = VisitExpressionList(m.Arguments, context);
                object quotedColName0 = Visit(m.Object, context);
                return new PartialSqlString(string.Format(GlobalSettings.Culture, "LEFT( {0},{1})= {2} ", quotedColName0
                                                          , args0[0].ToString()!.Length,
                                                          TypeConvert.TypeValueToDbValueStatement(args0[0], quotedIfNeed: true, context.EngineType)));
            }

            #endregion Mysql,其他数据库可能需要重写

            List<object> args = VisitExpressionList(m.Arguments, context);
            object quotedColName = Visit(m.Object, context);
            string statement;

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format(GlobalSettings.Culture, "ltrim(rtrim({0}))", quotedColName);
                    break;

                case "LTrim":
                    statement = string.Format(GlobalSettings.Culture, "ltrim({0})", quotedColName);
                    break;

                case "RTrim":
                    statement = string.Format(GlobalSettings.Culture, "rtrim({0})", quotedColName);
                    break;

                case "ToUpper":
                    statement = string.Format(GlobalSettings.Culture, "upper({0})", quotedColName);
                    break;

                case "ToLower":
                    statement = string.Format(GlobalSettings.Culture, "lower({0})", quotedColName);
                    break;

                case "StartsWith":
                    statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1} ", quotedColName,
                        SqlHelper.GetQuoted(args[0].ToString()!.ToUpper(GlobalSettings.Culture) + "%"));
                    break;

                case "EndsWith":
                    statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1}", quotedColName,
                        SqlHelper.GetQuoted("%" + args[0].ToString()!.ToUpper(GlobalSettings.Culture)));
                    break;

                case "Contains":
                    statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1}", quotedColName,
                        SqlHelper.GetQuoted("%" + args[0].ToString()!.ToUpper(GlobalSettings.Culture) + "%"));
                    break;

                case "Substring":
                    int startIndex = int.Parse(args[0].ToString()!, GlobalSettings.Culture) + 1;
                    if (args.Count == 2)
                    {
                        int length = int.Parse(args[1].ToString()!, GlobalSettings.Culture);
                        statement = string.Format(GlobalSettings.Culture, "substring({0} from {1} for {2})",
                                                  quotedColName,
                                                  startIndex,
                                                  length);
                    }
                    else
                        statement = string.Format(GlobalSettings.Culture, "substring({0} from {1})",
                                         quotedColName,
                                         startIndex);
                    break;

                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        private static bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object is MethodCallExpression methodCallExpression)
                return IsColumnAccess(methodCallExpression);

            return m.Object is MemberExpression exp
                && exp.Expression != null
                //&& _modelDefCollection.Any<ModelDef>(md => md.ModelType == exp.Expression.Type)
                && exp.Expression.NodeType == ExpressionType.Parameter;
        }

        private static string BindOperant(ExpressionType e) => e switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "MOD",
            ExpressionType.Coalesce => "COALESCE",
            _ => e.ToString(),
        };

        private static string RemoveQuoteFromAlias(string exp)
        {
            if ((exp.StartsWith("\"", GlobalSettings.Comparison) || exp.StartsWith("`", GlobalSettings.Comparison) || exp.StartsWith("'", GlobalSettings.Comparison))
                &&
                (exp.EndsWith("\"", GlobalSettings.Comparison) || exp.EndsWith("`", GlobalSettings.Comparison) || exp.EndsWith("'", GlobalSettings.Comparison)))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }

        private static bool IsTableField(Type type, object quotedExp)
        {
            //#if NETSTANDARD2_1
            //            string name = quotedExp.ToString()!.Replace(SqlHelper.QuotedChar, "", GlobalSettings.Comparison);
            //#endif
            //#if NETSTANDARD2_0
#pragma warning disable CA1307 // Specify StringComparison for clarity
            string name = quotedExp.ToString()!.Replace(SqlHelper.QuotedChar, "");
#pragma warning restore CA1307 // Specify StringComparison for clarity
                              //#endif

            EntityDef? entityDef = EntityDefFactory.GetDef(type);

            if (entityDef == null)
            {
                return false;
            }

            EntityPropertyDef? property = entityDef.GetPropertyDef(name);

            return property != null;
        }

        private static readonly object _trueExpression = new PartialSqlString("(1=1)");

        private static readonly object _falseExpression = new PartialSqlString("(1=0)");

        private static bool IsArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 2)
                    return true;
            }

            return false;
        }
    }
}