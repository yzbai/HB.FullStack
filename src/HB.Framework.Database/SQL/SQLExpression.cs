using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;
using System.Reflection;

namespace HB.Framework.Database.SQL
{
    public abstract class SQLExpression
    {
        protected string _sep;
        protected int _paramCounter;
        protected string _paramPlaceHolderPrefix;
        protected IDatabaseEntityDefFactory _entityDefFactory;

        public bool IsParameterized { get; set; }
        public bool PrefixFieldWithTableName { get; set; }
        public IList<KeyValuePair<string, object>> Params { get; set; }

        protected SQLExpression(IDatabaseEntityDefFactory modelDefFactory)
        {
            _entityDefFactory = modelDefFactory;
            _sep = string.Empty;
            _paramPlaceHolderPrefix = "_";
            PrefixFieldWithTableName = false;
            _paramCounter = 0;
            IsParameterized = true;

            Params = new List<KeyValuePair<string, object>>();
        }

        protected abstract IDatabaseEngine GetDatabaseEngine();

        #region 解析 lamda表达式

        protected internal virtual object Visit(Expression exp)
        {

            if (exp == null) return string.Empty;
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
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
                    return VisitBinary(exp as BinaryExpression);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit(exp as MemberInitExpression);
                default:
                    return exp.ToString();
            }
        }

        protected virtual object VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && _sep == " ")
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    object r = VisitMemberAccess(m);

                    if (!(r is PartialSqlString))
                    {
                        return r;
                    }

                    if (!m.Expression.Type.IsValueType)
                    {
                        return r.ToString();
                    }

                    return $"{r}={GetDatabaseEngine().GetDbValueStatement(true, needQuoted:true)}";
                }

            }
            return Visit(lambda.Body);
        }

        protected virtual object VisitBinary(BinaryExpression b)
        {
            object left, right;
            bool rightIsNull;
            var operand = BindOperant(b.NodeType);   //sep= " " ??
            if (operand == "AND" || operand == "OR")
            {
                var m = b.Left as MemberExpression;

                if (m != null && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    left = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetDatabaseEngine().GetDbValueStatement(true, needQuoted:true)));
                }
                else
                {
                    left = Visit(b.Left);
                }

                m = b.Right as MemberExpression;

                if (m != null && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    right = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetDatabaseEngine().GetDbValueStatement(true, needQuoted:true)));
                }
                else
                {
                    right = Visit(b.Right);
                }

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return new PartialSqlString(GetDatabaseEngine().GetDbValueStatement(result, needQuoted:true));
                }

                if (left as PartialSqlString == null)
                {
                    left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
                }

                if (right as PartialSqlString == null)
                {
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
                }
            }
            else
            {
                left = Visit(b.Left);
                right = Visit(b.Right);


                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return result;
                }
                else if (left as PartialSqlString == null)
                {
                    left = GetDatabaseEngine().GetDbValueStatement(left, needQuoted:true);
                }
                else if (right as PartialSqlString == null)
                {
                    if (!IsParameterized || right == null)
                    {
                        right = GetDatabaseEngine().GetDbValueStatement(right, needQuoted: true);
                    }
                    else
                    {
                        string paramPlaceholder = _paramPlaceHolderPrefix + _paramCounter++;
                        Params.Add(new KeyValuePair<string, object>(paramPlaceholder, right));
                        right = paramPlaceholder;
                    }
                }

            }
            //TODO: Test this. switch InvariantCultureIgnoreCase to OrdinalIgnoreCase
            rightIsNull = right.ToString().Equals("null", StringComparison.OrdinalIgnoreCase);
            if (operand == "=" && rightIsNull) operand = "is";
            else if (operand == "<>" && rightIsNull) operand = "is not";

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString(string.Format("{0}({1},{2})", operand, left, right));
                default:
                    return new PartialSqlString("(" + left + _sep + operand + _sep + right + ")");
            }
        }

        protected virtual object VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert ))
            {
                string memberName = m.Member.Name;
                Type modelType = m.Expression.Type;

                if (m.Expression.NodeType == ExpressionType.Convert)
                {
                    object obj = Visit(m.Expression);

                    if (obj is Type)
                    {
                        modelType = obj as Type;
                    }
                    else
                    {
                        //TODO: Test this;
                        return obj.GetType().GetTypeInfo().GetProperty(memberName).GetValue(obj);
                        //return obj.GetType().InvokeMember(memberName, System.Reflection.BindingFlags.GetProperty, null, obj, null);
                    }
                }

                DatabaseEntityDef entityDef = _entityDefFactory.Get(modelType);
                DatabaseEntityPropertyDef propertyDef = entityDef.GetProperty(m.Member.Name);

                string prefix = "";

                if (PrefixFieldWithTableName && !string.IsNullOrEmpty(entityDef.DbTableReservedName))
                {
                    prefix = entityDef.DbTableReservedName + ".";
                }

                if (propertyDef.PropertyType.GetTypeInfo().IsEnum)
                    return new EnumMemberAccess(prefix + propertyDef.DbReservedName, propertyDef.PropertyType);

                return new PartialSqlString(prefix + propertyDef.DbReservedName);
            }

            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        protected virtual object VisitMemberInit(MemberInitExpression exp)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke();
        }

        protected virtual object VisitNew(NewExpression nex)
        {
            // TODO : check !
            var member = Expression.Convert(nex, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                return getter();
            }
            catch (System.InvalidOperationException)
            { // FieldName ?
                List<Object> exprs = VisitExpressionList(nex.Arguments);
                StringBuilder r = new StringBuilder();
                foreach (Object e in exprs)
                {
                    r.AppendFormat("{0}{1}",
                                   r.Length > 0 ? "," : "",
                                   e);
                }
                return r.ToString();
            }

        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
            //return p.Name;
            return p.Type;
        }

        protected virtual object VisitConstant(ConstantExpression c)
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

        protected virtual object VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    var o = Visit(u.Operand);

                    if (o as PartialSqlString == null)
                        return !((bool)o);

                    if (IsTableField(u.Type, o))
                        o = o + "=" + GetDatabaseEngine().GetDbValueStatement(true, needQuoted: true);

                    return new PartialSqlString("NOT (" + o + ")");
                case ExpressionType.Convert:
                    if (u.Method != null)
                        return Expression.Lambda(u).Compile().DynamicInvoke();
                    break;
            }

            return Visit(u.Operand);

        }

        protected virtual object VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(SQLUtility))
                return VisitSqlMethodCall(m);

            if (IsArrayMethod(m))
                return VisitArrayMethodCall(m);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m);

            return Expression.Lambda(m).Compile().DynamicInvoke();
        }

        protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Object> list = new List<Object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                 original[i].NodeType == ExpressionType.NewArrayBounds)
                {

                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                    list.Add(Visit(original[i]));

            }
            return list;
        }

        protected virtual object VisitNewArray(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            StringBuilder r = new StringBuilder();
            foreach (Object e in exprs)
            {
                r.Append(r.Length > 0 ? "," + e : e);
            }

            return r.ToString();
        }

        protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }

        protected virtual object VisitArrayMethodCall(MethodCallExpression m)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    List<Object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args[1];

                    var memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression);

                    var member = Expression.Convert(memberExpr, typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = getter() as object[];

                    StringBuilder sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         GetDatabaseEngine().GetDbValueStatement(e, needQuoted: true));
                        }
                        else
                        {
                            var listArgs = e as IList<Object>;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         GetDatabaseEngine().GetDbValueStatement(el, needQuoted: true));
                            }
                        }
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        protected virtual object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":

                    var member = Expression.Convert(m.Arguments[1], typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = getter() as object[];

                    StringBuilder sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (!typeof(ICollection).GetTypeInfo().IsAssignableFrom(e.GetType()))
                        {
                            sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         GetDatabaseEngine().GetDbValueStatement(e, needQuoted: true));
                        }
                        else
                        {
                            var listArgs = e as ICollection;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         GetDatabaseEngine().GetDbValueStatement(el, needQuoted: true));
                            }
                        }
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn.ToString());
                    break;
                case "Desc":
                    statement = string.Format("{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format("{0} As {1}", quotedColName,
                        GetDatabaseEngine().GetQuotedStatement(RemoveQuoteFromAlias(args[0].ToString())));
                    break;
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                case "Distinct":
                    statement = string.Format("{0}({1}{2})",
                                         m.Method.Name,
                                         quotedColName,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                    break;
                case "Plain":
                    statement = quotedColName.ToString();
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
        {
            #region Mysql,其他数据库可能需要重写
            //TODO: Mysql,其他数据库可能需要重写
            if (m.Method.Name == "StartsWith")
            {
                List<Object> args0 = this.VisitExpressionList(m.Arguments);
                var quotedColName0 = Visit(m.Object);
                return new PartialSqlString(string.Format("LEFT( {0},{1})= {2} ", quotedColName0
                                                          , args0[0].ToString().Length,
                                                          GetDatabaseEngine().GetDbValueStatement(args0[0], needQuoted: true)));
            }

            #endregion

            List<Object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format("ltrim(rtrim({0}))", quotedColName);
                    break;
                case "LTrim":
                    statement = string.Format("ltrim({0})", quotedColName);
                    break;
                case "RTrim":
                    statement = string.Format("rtrim({0})", quotedColName);
                    break;
                case "ToUpper":
                    statement = string.Format("upper({0})", quotedColName);
                    break;
                case "ToLower":
                    statement = string.Format("lower({0})", quotedColName);
                    break;
                case "StartsWith":
                    statement = string.Format("upper({0}) like {1} ", quotedColName, GetDatabaseEngine().GetQuotedStatement(args[0].ToString().ToUpper() + "%"));
                    break;
                case "EndsWith":
                    statement = string.Format("upper({0}) like {1}", quotedColName, GetDatabaseEngine().GetQuotedStatement("%" + args[0].ToString().ToUpper()));
                    break;
                case "Contains":
                    statement = string.Format("upper({0}) like {1}", quotedColName, GetDatabaseEngine().GetQuotedStatement("%" + args[0].ToString().ToUpper() + "%"));
                    break;
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        statement = string.Format("substring({0} from {1} for {2})",
                                                  quotedColName,
                                                  startIndex,
                                                  length);
                    }
                    else
                        statement = string.Format("substring({0} from {1})",
                                         quotedColName,
                                         startIndex);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        private bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object != null && m.Object as MethodCallExpression != null)
                return IsColumnAccess(m.Object as MethodCallExpression);

            var exp = m.Object as MemberExpression;
            return exp != null
                && exp.Expression != null
                //&& _modelDefCollection.Any<ModelDef>(md => md.ModelType == exp.Expression.Type)
                && exp.Expression.NodeType == ExpressionType.Parameter;
        }

        protected virtual string BindOperant(ExpressionType e)
        {

            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        protected string RemoveQuoteFromAlias(string exp)
        {

            if ((exp.StartsWith("\"") || exp.StartsWith("`") || exp.StartsWith("'"))
                &&
                (exp.EndsWith("\"") || exp.EndsWith("`") || exp.EndsWith("'")))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }

        protected bool IsTableField(Type type, object quotedExp)
        {
            string name = quotedExp.ToString().Replace(GetDatabaseEngine().QuotedChar, "");

            DatabaseEntityDef entityDef = _entityDefFactory.Get(type);

            DatabaseEntityPropertyDef property = entityDef.GetProperty(name);

            if (property == null)
            {
                return false;
            }

            return property.IsTableProperty;
        }

        protected object GetTrueExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetDatabaseEngine().GetDbValueStatement(true, needQuoted: true), GetDatabaseEngine().GetDbValueStatement(true, needQuoted: true)));
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetDatabaseEngine().GetDbValueStatement(true, needQuoted: true), GetDatabaseEngine().GetDbValueStatement(false, needQuoted: true)));
        }

        private bool IsArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 2)
                    return true;
            }

            return false;
        }

        #endregion 
    }
}
