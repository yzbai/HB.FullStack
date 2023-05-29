using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common.Convert;

namespace System
{
    public static class CommonReflectionInfos
    {
        public static readonly MethodInfo InvariantCultureMethodInfo = typeof(CultureInfo).GetProperty(nameof(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static)!.GetGetMethod()!;

        public static readonly MethodInfo GetTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!;

        public static readonly MethodInfo ArraySetValueMethod = typeof(Array).GetMethod(nameof(Array.SetValue), new Type[] { typeof(object), typeof(int) })!;
        public static readonly MethodInfo ArrayGetValueMethod = typeof(Array).GetMethod(nameof(Array.GetValue), new Type[] { typeof(int) })!;
        public static readonly MethodInfo ArrayGetLengthMethod = typeof(Array).GetMethod(nameof(Array.GetLength), new Type[] { typeof(int) })!;

        public static readonly MethodInfo StringConcatMethod = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object), typeof(object) })!;
        public static readonly MethodInfo ConvertToStringMethod = typeof(StringConvertCenter).GetMethod(nameof(StringConvertCenter.ToString), new Type[] { typeof(object), typeof(Type), typeof(StringConvertPurpose) })!;

        public static readonly ConstructorInfo StringListConstructorInfo = typeof(List<>).MakeGenericType(typeof(string)).GetConstructor(Array.Empty<Type>())!;

        public static readonly MethodInfo StringListAddMethod = typeof(List<>).MakeGenericType(typeof(string)).GetMethod("Add")!;

        public static readonly MethodInfo IEnumerableGetEnumeratorMethod = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator))!;
        public static readonly MethodInfo EnumeratorGetCurrentMethod = typeof(IEnumerator).GetMethod("get_Current")!;
        public static readonly MethodInfo EnumeratorMoveNextMethod = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!;

        public static readonly MethodInfo DataReaderGetItemMethod = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)!
            .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))!
            .Select(p => p.GetGetMethod()).First()!;

        public static readonly MethodInfo EnumParseMethod = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) })!;
        public static readonly MethodInfo ObjectToStringMethod = typeof(object).GetMethod(nameof(object.ToString))!;
        public static readonly FieldInfo DbNullValueFiled = typeof(DBNull).GetField("Value")!;

        public static readonly ConstructorInfo PropertyValueConstructorInfo = typeof(HB.FullStack.Common.Meta.PropertyNameValue).GetConstructor(new Type[] { typeof(string), typeof(object) })!;
    }
}
