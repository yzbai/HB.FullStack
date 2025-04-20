using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HB.FullStack.Common.NotUsed
{
    //使用Expression可以提速的本质原因，就是提前走了一遍，节省了中间步骤。
    //应该混存Compile后的Delegate
    public static class ExpressionUtils
    {
        public static Func<object, object?> CreatePropertyGetDeleagteByExpression(Type declareType, string propertyName)
        {
            // (object instance) => (object)((declaringType)instance).propertyName

            var param_instance = Expression.Parameter(typeof(object));
            var body_objToType = Expression.Convert(param_instance, declareType);
            var body_getTypeProperty = Expression.Property(body_objToType, propertyName);
            var body_return = Expression.Convert(body_getTypeProperty, typeof(object));
            return Expression.Lambda<Func<object, object>>(body_return, param_instance).Compile();
        }

        public static Action<object, object?> CreatePropertySetDelagateByExpression(PropertyInfo property)
        {
            // (object instance, object value) => 
            //     ((instanceType)instance).Set_XXX((propertyType)value)

            //声明方法需要的参数
            var param_instance = Expression.Parameter(typeof(object));
            var param_value = Expression.Parameter(typeof(object));

            var body_instance = Expression.Convert(param_instance, property.DeclaringType!);
            var body_value = Expression.Convert(param_value, property.PropertyType);
            var body_call = Expression.Call(body_instance, property.GetSetMethod()!, body_value);

            return Expression.Lambda<Action<object, object?>>(body_call, param_instance, param_value).Compile();
        }
    }
}
