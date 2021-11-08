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
        
        public static object Visit(Expression? exp, SQLExpressionVisitorContenxt context)
        {
            if (exp == null) return string.Empty;

            return exp.NodeType switch
            {
                ExpressionType.Lambda => VisitLambda((LambdaExpression)exp, context),
                ExpressionType.MemberAccess => VisitMemberAccess((MemberExpression)exp, context),
                ExpressionType.Constant => VisitConstant((ConstantExpression)exp),
                
                ExpressionType.Add 
                    or ExpressionType.AddChecked 
                    or ExpressionType.Subtract 
                    or ExpressionType.SubtractChecked 
                    or ExpressionType.Multiply 
                    or ExpressionType.MultiplyChecked 
                    or ExpressionType.Divide 
                    or ExpressionType.Modulo 
                    or ExpressionType.And 
                    or ExpressionType.AndAlso 
                    or ExpressionType.Or 
                    or ExpressionType.OrElse 
                    or ExpressionType.LessThan 
                    or ExpressionType.LessThanOrEqual 
                    or ExpressionType.GreaterThan 
                    or ExpressionType.GreaterThanOrEqual 
                    or ExpressionType.Equal 
                    or ExpressionType.NotEqual 
                    or ExpressionType.Coalesce 
                    or ExpressionType.ArrayIndex 
                    or ExpressionType.RightShift 
                    or ExpressionType.LeftShift 
                    or ExpressionType.ExclusiveOr 
                    => VisitBinary((BinaryExpression)exp, context),

                ExpressionType.Negate 
                    or ExpressionType.NegateChecked 
                    or ExpressionType.Not 
                    or ExpressionType.Convert 
                    or ExpressionType.ConvertChecked 
                    or ExpressionType.ArrayLength 
                    or ExpressionType.Quote 
                    or ExpressionType.TypeAs 
                    => VisitUnary((UnaryExpression)exp, context),

                ExpressionType.Parameter => VisitParameter((ParameterExpression)exp),
                ExpressionType.Call => VisitMethodCall((MethodCallExpression)exp, context),
                ExpressionType.New => VisitNew((NewExpression)exp, context),
                ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds => VisitNewArray((NewArrayExpression)exp, context),
                ExpressionType.MemberInit => VisitMemberInit((MemberInitExpression)exp),
                
                _ => exp.ToString(),
            };
        }

        /// <summary>
        /// VisitLambda
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        
        private static object VisitLambda(LambdaExpression lambda, SQLExpressionVisitorContenxt context)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && context.Seperator == " ")
            {
                MemberExpression m = (MemberExpression)lambda.Body;

                if (m.Expression != null)
                {
                    object r = VisitMemberAccess(m, context);

                    if (r is not PartialSqlString)
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
        
        private static object VisitBinary(BinaryExpression b, SQLExpressionVisitorContenxt context)
        {
            object left;
            object right;
            bool rightIsNull;
            string operand = BindOperant(b.NodeType);

            if (operand == "AND" || operand == "OR")
            {
                //说明是两部分
                //先求left
                if (b.Left is MemberExpression m && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    //left = new PartialSqlString(string.Format(GlobalSettings.Culture, "{0}={1}", VisitMemberAccess(m, context), context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)));
                    left = new PartialSqlString($"{VisitMemberAccess(m, context)}=1");
                }
                else
                {
                    left = Visit(b.Left, context);
                }

                //再求right
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

                    string paramPlaceholder = context.GetNextParamPlaceholder();
                    object paramValue = TypeConvert.TypeValueToDbValue(result, null, context.EngineType);

                    context.AddParameter(paramPlaceholder, paramValue);

                    return new PartialSqlString(paramPlaceholder);
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

                if (left is EnumMemberAccess enumMemberAccess)
                {
                    //将right转为enum对应的字符串
                    right = Enum.ToObject(enumMemberAccess.EnumType, right).ToString();
                }

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    object result = Expression.Lambda(b).Compile().DynamicInvoke()!;

                    string paramPlaceholder = context.GetNextParamPlaceholder();
                    object paramValue = TypeConvert.TypeValueToDbValue(result, null, context.EngineType);

                    context.AddParameter(paramPlaceholder, paramValue);

                    return new PartialSqlString(paramPlaceholder);
                }
                else if (left as PartialSqlString == null)
                {
                    string paramPlaceholder = context.GetNextParamPlaceholder();
                    object paramValue = TypeConvert.TypeValueToDbValue(left, null, context.EngineType);

                    context.AddParameter(paramPlaceholder, paramValue);

                    left = paramPlaceholder;
                }
                else if (right as PartialSqlString == null)
                {
                    string paramPlaceholder = context.GetNextParamPlaceholder();
                    object paramValue = TypeConvert.TypeValueToDbValue(right, GetPropertyDef(b.Left as MemberExpression), context.EngineType);

                    context.AddParameter(paramPlaceholder, paramValue);

                    right = paramPlaceholder;
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
                    ?? throw DatabaseExceptions.EntityError(entityDef.EntityFullName, m.Member.Name, "Lack property definition");

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
        
        private static object VisitUnary(UnaryExpression u, SQLExpressionVisitorContenxt context)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    object o = Visit(u.Operand, context);

                    if (o as PartialSqlString == null)
                        return !(bool)o;

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

                    EntityPropertyDef? propertyDef = GetPropertyDef(m);

                    foreach (object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            AddParameter(context, sIn, propertyDef, e);
                        }
                        else
                        {
                            IList<object> listArgs = (IList<object>)e;

                            foreach (object el in listArgs)
                            {
                                AddParameter(context, sIn, propertyDef, el);
                            }
                        }
                    }

                    statement = string.Format(GlobalSettings.Culture, "{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);

            static void AddParameter(SQLExpressionVisitorContenxt context, StringBuilder sIn, EntityPropertyDef? propertyDef, object e)
            {
                string paramPlaceHoder = context.GetNextParamPlaceholder();
                object paramValue = propertyDef == null ? e : TypeConvert.TypeValueToDbValue(e, propertyDef, context.EngineType);

                context.AddParameter(paramPlaceHoder, paramValue);

                sIn.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}{1}",
                    sIn.Length > 0 ? "," : "",
                    paramPlaceHoder);
            }
        }

        /// <summary>
        /// VisitSqlMethodCall
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        
        private static object VisitSqlMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            List<object> args = VisitExpressionList(m.Arguments, context);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":
                    //UnaryExpression member = Expression.Convert(m.Arguments[2], typeof(object));
                    //Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
                    //Func<object> getter = lambda.Compile();

                    //IEnumerable inArgs = (IEnumerable)getter();

                    bool returnByOrder = Convert.ToBoolean(args[0], CultureInfo.InvariantCulture);
                    IEnumerable inArgs = (IEnumerable)args[1];

                    List<string> sIn = new List<string>();
                    TypeInfo collectionTypeInfo = typeof(ICollection).GetTypeInfo();
                    EntityPropertyDef? propertyDef = m.Arguments[0] is MemberExpression memExp ? GetPropertyDef(memExp) : null;

                    foreach (object e in inArgs)
                    {
                        if (!collectionTypeInfo.IsAssignableFrom(e.GetType()))
                        {
                            AddParameter(context, sIn, propertyDef, e);
                        }
                        else
                        {
                            ICollection listArgs = (ICollection)e;
                            foreach (object el in listArgs)
                            {
                                AddParameter(context, sIn, propertyDef, el);
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

                    if (returnByOrder)
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

            static void AddParameter(SQLExpressionVisitorContenxt context, List<string> sIn, EntityPropertyDef? propertyDef, object originValue)
            {
                string paramPlaceholder = context.GetNextParamPlaceholder();
                object paramValue = propertyDef == null ? originValue : TypeConvert.TypeValueToDbValue(originValue, propertyDef, context.EngineType);

                context.AddParameter(paramPlaceholder, paramValue);

                sIn.Add(paramPlaceholder);
            }
        }

        /// <summary>
        /// 字符串型Member的字符串操作
        /// </summary>
        /// <param name="m"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        
        private static object VisitColumnAccessMethod(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            if (m.Method.Name == "StartsWith")
            {
                //TODO: Mysql,其他数据库可能需要重写
                if (context.EngineType != Engine.EngineType.MySQL)
                {
                    throw DatabaseExceptions.NotSupportYet("目前仅支持StarWith的MySql数据库版本", context.EngineType);
                }

                List<object> args0 = VisitExpressionList(m.Arguments, context);
                object quotedColName0 = Visit(m.Object, context);

                EntityPropertyDef? propertyDef = GetPropertyDef(m);
                string paramPlaceholder = context.GetNextParamPlaceholder();
                object paramValue = propertyDef == null ?
                    args0[0] :
                    TypeConvert.TypeValueToDbValue(args0[0], propertyDef, context.EngineType);

                context.AddParameter(paramPlaceholder, paramValue);

                return new PartialSqlString(string.Format(GlobalSettings.Culture,
                    "LEFT( {0},{1})= {2} ",
                    quotedColName0,
                    args0[0].ToString()!.Length,
                    paramPlaceholder));
            }

            throw DatabaseExceptions.NotSupportYet("暂时还不支持其他操作", context.EngineType);

            //List<object> args = VisitExpressionList(m.Arguments, context);
            //object quotedColName = Visit(m.Object, context);
            //string statement;

            //switch (m.Method.Name)
            //{
            //    case "Trim":
            //        statement = string.Format(GlobalSettings.Culture, "ltrim(rtrim({0}))", quotedColName);
            //        break;

            //    case "LTrim":
            //        statement = string.Format(GlobalSettings.Culture, "ltrim({0})", quotedColName);
            //        break;

            //    case "RTrim":
            //        statement = string.Format(GlobalSettings.Culture, "rtrim({0})", quotedColName);
            //        break;

            //    case "ToUpper":
            //        statement = string.Format(GlobalSettings.Culture, "upper({0})", quotedColName);
            //        break;

            //    case "ToLower":
            //        statement = string.Format(GlobalSettings.Culture, "lower({0})", quotedColName);
            //        break;

            //    case "StartsWith":
            //        statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1} ", quotedColName,
            //            SqlHelper.GetQuoted(args[0].ToString()!.ToUpper(GlobalSettings.Culture) + "%"));
            //        break;

            //    case "EndsWith":
            //        statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1}", quotedColName,
            //            SqlHelper.GetQuoted("%" + args[0].ToString()!.ToUpper(GlobalSettings.Culture)));
            //        break;

            //    case "Contains":
            //        statement = string.Format(GlobalSettings.Culture, "upper({0}) like {1}", quotedColName,
            //            SqlHelper.GetQuoted("%" + args[0].ToString()!.ToUpper(GlobalSettings.Culture) + "%"));
            //        break;

            //    case "Substring":
            //        int startIndex = int.Parse(args[0].ToString()!, GlobalSettings.Culture) + 1;
            //        if (args.Count == 2)
            //        {
            //            int length = int.Parse(args[1].ToString()!, GlobalSettings.Culture);
            //            statement = string.Format(GlobalSettings.Culture, "substring({0} from {1} for {2})",
            //                                      quotedColName,
            //                                      startIndex,
            //                                      length);
            //        }
            //        else
            //            statement = string.Format(GlobalSettings.Culture, "substring({0} from {1})",
            //                             quotedColName,
            //                             startIndex);
            //        break;

            //    default:
            //        throw new NotSupportedException();
            //}
            //return new PartialSqlString(statement);
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
            string name = quotedExp.ToString()!.Replace(SqlHelper.QUOTED_CHAR, "");
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

        private static EntityPropertyDef? GetPropertyDef(MemberExpression? memberExpression)
        {
            return EntityDefFactory.GetDef(memberExpression?.Expression.Type)?.GetPropertyDef(memberExpression!.Member.Name);
        }

        private static EntityPropertyDef? GetPropertyDef(MethodCallExpression methodCallExpression)
        {
            return GetPropertyDef(methodCallExpression.Object as MemberExpression);
        }
    }
}