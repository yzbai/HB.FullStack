#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace HB.FullStack.Database.SQL
{
    //http://blogs.msdn.com/b/mattwar/archive/2007/07/31/linq-building-an-iqueryable-provider-part-ii.aspx
    internal abstract class ExpressionVisitor
    {
        [return: NotNullIfNotNull("exp")]
        protected virtual Expression? Visit(Expression? exp)
        {
            if (exp == null)
                return exp;

            return exp.NodeType switch
            {
                ExpressionType.Negate 
                    or ExpressionType.NegateChecked 
                    or ExpressionType.Not 
                    or ExpressionType.Convert 
                    or ExpressionType.ConvertChecked 
                    or ExpressionType.ArrayLength 
                    or ExpressionType.Quote 
                    or ExpressionType.TypeAs 
                    => VisitUnary((UnaryExpression)exp),

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
                    => VisitBinary((BinaryExpression)exp),

                ExpressionType.Lambda => VisitLambda((LambdaExpression)exp),
                ExpressionType.TypeIs => VisitTypeIs((TypeBinaryExpression)exp),
                ExpressionType.Conditional => VisitConditional((ConditionalExpression)exp),
                ExpressionType.Constant => VisitConstant((ConstantExpression)exp),
                ExpressionType.Parameter => VisitParameter((ParameterExpression)exp),
                ExpressionType.MemberAccess => VisitMemberAccess((MemberExpression)exp),
                ExpressionType.Call => VisitMethodCall((MethodCallExpression)exp),
                ExpressionType.New => VisitNew((NewExpression)exp),
                ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds => VisitNewArray((NewArrayExpression)exp),
                ExpressionType.Invoke => VisitInvocation((InvocationExpression)exp),
                ExpressionType.MemberInit => VisitMemberInit((MemberInitExpression)exp),
                ExpressionType.ListInit => VisitListInit((ListInitExpression)exp),
                
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// VisitBinding
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            return binding.BindingType switch
            {
                MemberBindingType.Assignment => VisitMemberAssignment((MemberAssignment)binding),
                MemberBindingType.MemberBinding => VisitMemberMemberBinding((MemberMemberBinding)binding),
                MemberBindingType.ListBinding => VisitMemberListBinding((MemberListBinding)binding),
                _ => throw new NotSupportedException()
            };
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression? operand = Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression? left = Visit(b.Left);
            Expression? right = Visit(b.Right);
            Expression? conversion = Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression? expr = Visit(b.Expression);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression? test = Visit(c.Test);
            Expression? ifTrue = Visit(c.IfTrue);
            Expression? ifFalse = Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression? exp = Visit(m.Expression);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression? obj = Visit(m.Object);
            IEnumerable<Expression> args = VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression>? list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression? p = Visit(original[i]);

                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);

                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression? e = Visit(assignment.Expression);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding>? list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit>? list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression? body = Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                if (nex.Members != null)
                    return Expression.New(nex.Constructor!, args, nex.Members);
                else
                    return Expression.New(nex.Constructor!, args);
            }
            return nex;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType()!, exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType()!, exprs);
                }
            }
            return na;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitExpressionList(iv.Arguments);
            Expression? expr = Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }
    }
}