#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using HB.FullStack.Database.Entities;
using System.Reflection;
using System.Globalization;

namespace HB.FullStack.Database.SQL
{
    internal static class SQLExpressionVisitor
    {
        public static object Visit(Expression exp, SQLExpressionVisitorContenxt context)
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
                        return r.ToString();
                    }

                    //return $"{r}={context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)}";
                    return $"{r}=1";
                }

            }
            return Visit(lambda.Body, context);
        }

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
                    //left = new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "{0}={1}", VisitMemberAccess(m, context), context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)));
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
                    object result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return new PartialSqlString(context.DatabaesEngine.GetDbValueStatement(result, needQuoted: true));
                }

                if (left as PartialSqlString == null)
                {
                    left = ((bool)left) ? GetTrueExpression(context) : GetFalseExpression(context);
                }

                if (right as PartialSqlString == null)
                {
                    right = ((bool)right) ? GetTrueExpression(context) : GetFalseExpression(context);
                }
            }
            else
            {
                left = Visit(b.Left, context);
                right = Visit(b.Right, context);


                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    object result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return result;
                }
                else if (left as PartialSqlString == null)
                {
                    left = context.DatabaesEngine.GetDbValueStatement(left, needQuoted: true);
                }
                else if (right as PartialSqlString == null)
                {
                    if (!context.IsParameterized /*|| right == null*/)
                    {
                        right = context.DatabaesEngine.GetDbValueStatement(right, needQuoted: true);
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
            rightIsNull = right.ToString().Equals("null", GlobalSettings.ComparisonIgnoreCase);

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
                "MOD" => new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", operand, left, right)),
                "COALESCE" => new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", operand, left, right)),
                _ => new PartialSqlString("(" + left + context.Seperator + operand + context.Seperator + right + ")")
            };
        }

        private static object VisitMemberAccess(MemberExpression m, SQLExpressionVisitorContenxt context)
        {
            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
            {
                string memberName = m.Member.Name;
                Type modelType = m.Expression.Type;

                if (m.Expression.NodeType == ExpressionType.Convert)
                {
                    object obj = Visit(m.Expression, context);

                    if (obj is Type type)
                    {
                        modelType = type;
                    }
                    else
                    {
                        //TODO: Test this;
                        return obj.GetType().GetTypeInfo().GetProperty(memberName).GetValue(obj);
                        //return obj.GetType().InvokeMember(memberName, System.Reflection.BindingFlags.GetProperty, null, obj, null);
                    }
                }

                DatabaseEntityDef entityDef = context.EntityDefFactory.GetDef(modelType);
                DatabaseEntityPropertyDef propertyDef = entityDef.GetProperty(m.Member.Name)
                    ?? throw new DatabaseException($"Lack property definition: {m.Member.Name} of Entity:{entityDef.EntityFullName}");

                string prefix = "";

                if (context.PrefixFieldWithTableName && !string.IsNullOrEmpty(entityDef.DbTableReservedName))
                {
                    prefix = entityDef.DbTableReservedName + ".";
                }

                if (propertyDef.PropertyInfo.PropertyType.IsEnum)
                    return new EnumMemberAccess(prefix + propertyDef.DbReservedName, propertyDef.PropertyInfo.PropertyType);

                return new PartialSqlString(prefix + propertyDef.DbReservedName);
            }

            var member = Expression.Convert(m, typeof(object));
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        private static object VisitMemberInit(MemberInitExpression exp/*, SQLExpressionVisitorContenxt context*/)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke();
        }

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
                    r.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}",
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
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                return new PartialSqlString("null");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
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

        private static object VisitUnary(UnaryExpression u, SQLExpressionVisitorContenxt context)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    object o = Visit(u.Operand, context);

                    if (o as PartialSqlString == null)
                        return !((bool)o);

                    if (IsTableField(u.Type, o, context))
                        o = o + "=" + context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true);

                    return new PartialSqlString("NOT (" + o + ")");
                case ExpressionType.Convert:
                    if (u.Method != null)
                        return Expression.Lambda(u).Compile().DynamicInvoke();
                    break;
            }

            return Visit(u.Operand, context);

        }

        private static object VisitMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            if (m.Method.DeclaringType == typeof(SQLUtil))
                return VisitSqlMethodCall(m, context);

            if (IsArrayMethod(m))
                return VisitArrayMethodCall(m, context);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m, context);

            return Expression.Lambda(m).Compile().DynamicInvoke();
        }

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

        private static List<object> VisitNewArrayFromExpressionList(NewArrayExpression na, SQLExpressionVisitorContenxt context)
        {

            List<object> exprs = VisitExpressionList(na.Expressions, context);
            return exprs;
        }

        private static object VisitArrayMethodCall(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = VisitExpressionList(m.Arguments, context);
                    object quotedColName = args[1];

                    var memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression);

                    var member = Expression.Convert(memberExpr, typeof(object));
                    Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    object[] inArgs = (object[])getter();

                    StringBuilder sIn = new StringBuilder();
                    foreach (object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            sIn.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         context.DatabaesEngine.GetDbValueStatement(e, needQuoted: true));
                        }
                        else
                        {
                            IList<object> listArgs = (IList<object>)e;

                            foreach (object el in listArgs)
                            {
                                sIn.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         context.DatabaesEngine.GetDbValueStatement(el, needQuoted: true));
                            }
                        }
                    }

                    statement = string.Format(CultureInfo.InvariantCulture, "{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

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

                    object[] inArgs = (object[])getter();

                    List<string> sIn = new List<string>();

                    foreach (object e in inArgs)
                    {
                        if (!typeof(ICollection).GetTypeInfo().IsAssignableFrom(e.GetType()))
                        {
                            sIn.Add(context.DatabaesEngine.GetDbValueStatement(e, needQuoted: true));
                        }
                        else
                        {
                            ICollection listArgs = (ICollection)e;
                            foreach (object el in listArgs)
                            {
                                sIn.Add(context.DatabaesEngine.GetDbValueStatement(el, needQuoted: true));
                            }
                        }
                    }

                    if (sIn.Count == 0)
                    {
                        //防止集合为空
                        sIn.Add("null");
                    }

                    string joinedSIn = sIn.ToJoinedString(",");

                    statement = string.Format(CultureInfo.InvariantCulture, "{0} {1} ({2})", quotedColName, m.Method.Name, joinedSIn);

                    if (Convert.ToBoolean(args[0], CultureInfo.InvariantCulture))
                    {
                        //TODO: Move to SQLBuilder
                        if (context.DatabaesEngine.EngineType == Engine.DatabaseEngineType.MySQL)
                        {
                            context.OrderByStatementBySQLUtilIn = string.Format(CultureInfo.InvariantCulture, " ORDER BY FIELD({0}, {1}) ", quotedColName, joinedSIn);
                        }
                        else if (context.DatabaesEngine.EngineType == Engine.DatabaseEngineType.SQLite)
                        {
                            StringBuilder orderCaseBuilder = new StringBuilder();

                            for (int i = 0; i < sIn.Count; ++i)
                            {
                                orderCaseBuilder.Append($" when {sIn[i]} THEN {i} ");
                            }

                            context.OrderByStatementBySQLUtilIn = $" ORDER BY CASE {quotedColName} {orderCaseBuilder} END ";

                        }
                    }

                    break;
                case "Desc":
                    statement = string.Format(CultureInfo.InvariantCulture, "{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format(CultureInfo.InvariantCulture, "{0} As {1}", quotedColName,
                        context.DatabaesEngine.GetQuotedStatement(RemoveQuoteFromAlias(args[0].ToString())));
                    break;
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                case "Distinct":
                    statement = string.Format(CultureInfo.InvariantCulture, "{0}({1}{2})",
                                         m.Method.Name,
                                         quotedColName,
                                         args.Count == 1 ? string.Format(CultureInfo.InvariantCulture, ",{0}", args[0]) : "");
                    break;
                case "Plain":
                    statement = quotedColName.ToString();
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        private static object VisitColumnAccessMethod(MethodCallExpression m, SQLExpressionVisitorContenxt context)
        {
            #region Mysql,其他数据库可能需要重写
            //TODO: Mysql,其他数据库可能需要重写
            if (m.Method.Name == "StartsWith")
            {
                List<object> args0 = VisitExpressionList(m.Arguments, context);
                object quotedColName0 = Visit(m.Object, context);
                return new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "LEFT( {0},{1})= {2} ", quotedColName0
                                                          , args0[0].ToString().Length,
                                                          context.DatabaesEngine.GetDbValueStatement(args0[0], needQuoted: true)));
            }

            #endregion

            List<object> args = VisitExpressionList(m.Arguments, context);
            object quotedColName = Visit(m.Object, context);
            string statement;

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format(CultureInfo.InvariantCulture, "ltrim(rtrim({0}))", quotedColName);
                    break;
                case "LTrim":
                    statement = string.Format(CultureInfo.InvariantCulture, "ltrim({0})", quotedColName);
                    break;
                case "RTrim":
                    statement = string.Format(CultureInfo.InvariantCulture, "rtrim({0})", quotedColName);
                    break;
                case "ToUpper":
                    statement = string.Format(CultureInfo.InvariantCulture, "upper({0})", quotedColName);
                    break;
                case "ToLower":
                    statement = string.Format(CultureInfo.InvariantCulture, "lower({0})", quotedColName);
                    break;
                case "StartsWith":
                    statement = string.Format(CultureInfo.InvariantCulture, "upper({0}) like {1} ", quotedColName,
                        context.DatabaesEngine.GetQuotedStatement(args[0].ToString().ToUpper(CultureInfo.InvariantCulture) + "%"));
                    break;
                case "EndsWith":
                    statement = string.Format(CultureInfo.InvariantCulture, "upper({0}) like {1}", quotedColName,
                        context.DatabaesEngine.GetQuotedStatement("%" + args[0].ToString().ToUpper(CultureInfo.InvariantCulture)));
                    break;
                case "Contains":
                    statement = string.Format(CultureInfo.InvariantCulture, "upper({0}) like {1}", quotedColName,
                        context.DatabaesEngine.GetQuotedStatement("%" + args[0].ToString().ToUpper(CultureInfo.InvariantCulture) + "%"));
                    break;
                case "Substring":
                    int startIndex = int.Parse(args[0].ToString(), CultureInfo.InvariantCulture) + 1;
                    if (args.Count == 2)
                    {
                        int length = int.Parse(args[1].ToString(), CultureInfo.InvariantCulture);
                        statement = string.Format(CultureInfo.InvariantCulture, "substring({0} from {1} for {2})",
                                                  quotedColName,
                                                  startIndex,
                                                  length);
                    }
                    else
                        statement = string.Format(CultureInfo.InvariantCulture, "substring({0} from {1})",
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
            if (m.Object != null && m.Object as MethodCallExpression != null)
                return IsColumnAccess((MethodCallExpression)m.Object);

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

        private static bool IsTableField(Type type, object quotedExp, SQLExpressionVisitorContenxt context)
        {
            string name = quotedExp.ToString().Replace(context.DatabaesEngine.QuotedChar, "", GlobalSettings.Comparison);

            DatabaseEntityDef entityDef = context.EntityDefFactory.GetDef(type);

            DatabaseEntityPropertyDef? property = entityDef.GetProperty(name);

            if (property == null)
            {
                return false;
            }

            return property.IsTableProperty;
        }

        private static object GetTrueExpression(SQLExpressionVisitorContenxt context)
        {
            return new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "({0}={1})",
                context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true),
                context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true)));
        }

        private static object GetFalseExpression(SQLExpressionVisitorContenxt context)
        {
            return new PartialSqlString(string.Format(CultureInfo.InvariantCulture, "({0}={1})",
                context.DatabaesEngine.GetDbValueStatement(true, needQuoted: true),
                context.DatabaesEngine.GetDbValueStatement(false, needQuoted: true)));
        }

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
