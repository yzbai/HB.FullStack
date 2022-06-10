#nullable disable
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ClassLibrary1
{

    public class MySqlGuidTypeHandler : ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            return new Guid((byte[])value);
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {

        }
    }

    public class DateTimeOffsetTypeHandler : ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {

        }

        public object Parse(Type destinationType, object value)
        {
            Type dbValueType = value.GetType();

            if (dbValueType == typeof(string))
            {
                return DateTimeOffset.Parse(value.ToString(), GlobalSettings.Culture);
            }
            else if (dbValueType == typeof(long))
            {
                return new DateTimeOffset((long)value, TimeSpan.Zero);
            }
            else
            {
                return new DateTimeOffset((DateTime)value, TimeSpan.Zero);
            }

        }
    }


    public static class TypeHandlerHelper
    {

        static TypeHandlerHelper()
        {

        }

        internal static bool HasTypeHandler(Type type) => TypeHandlers.ContainsKey(type);

        public static Dictionary<Type, ITypeHandler> TypeHandlers = new Dictionary<Type, ITypeHandler>();

        public static void AddTypeHandlerImpl(Type type, ITypeHandler handler, bool clone)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            Type secondary = null;
            if (type.IsValueType)
            {
                var underlying = Nullable.GetUnderlyingType(type);
                if (underlying == null)
                {
                    secondary = typeof(Nullable<>).MakeGenericType(type); // the Nullable<T>
                    // type is already the T
                }
                else
                {
                    secondary = type; // the Nullable<T>
                    type = underlying; // the T
                }
            }

            var snapshot = TypeHandlers;
            if (snapshot.TryGetValue(type, out ITypeHandler oldValue) && handler == oldValue) return; // nothing to do

            var newCopy = clone ? new Dictionary<Type, ITypeHandler>(snapshot) : snapshot;
            typeof(TypeHandlerCache<>).MakeGenericType(type).GetMethod(nameof(TypeHandlerCache<int>.SetHandler), BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { handler });
            if (secondary != null)
            {
                typeof(TypeHandlerCache<>).MakeGenericType(secondary).GetMethod(nameof(TypeHandlerCache<int>.SetHandler), BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { handler });
            }

            if (handler == null)
            {
                newCopy.Remove(type);
                if (secondary != null) newCopy.Remove(secondary);
            }
            else
            {
                newCopy[type] = handler;
                if (secondary != null) newCopy[secondary] = handler;
            }
            TypeHandlers = newCopy;
        }

    }
}
#nullable restore