using System.Reflection;
using System.Reflection.Emit;

namespace System
{
    /// <summary>
    /// 一些常见的Emit组合
    /// </summary>
    public static class ILGeneratorExtensions
    {
        public static void ThrowIfArgumentIsNull(this ILGenerator il, byte argumentNumber, string? paramName, string? message)
        {
            Label inputObjectNotNullLabel = il.DefineLabel();

            //TODO: if use this, caused NullReferenceException throw? do not know why?
            //il.Emit(OpCodes.Ldarga_S, argumentNumber);//, argumentNumber);

            il.Emit(argumentNumber switch
            {
                0 => OpCodes.Ldarg_0,
                1 => OpCodes.Ldarg_1,
                2 => OpCodes.Ldarg_2,
                3 => OpCodes.Ldarg_3,
                _ => throw new NotImplementedException()
            });

            il.Emit(OpCodes.Brtrue_S, inputObjectNotNullLabel);

            il.ThrowArgumentNullException(paramName, message);

            il.MarkLabel(inputObjectNotNullLabel);
        }

        public static void ThrowArgumentException(this ILGenerator il, string message)
        {
            il.Emit(OpCodes.Ldstr, message);
            var ctor = typeof(ArgumentException).GetConstructor([typeof(string)])!;
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Throw);
        }

        public static void ThrowArgumentNullException(this ILGenerator il, string? paramName, string? message)
        {
            il.Emit(OpCodes.Ldstr, paramName ?? "");
            il.Emit(OpCodes.Ldstr, message ?? "");
            var ctor = typeof(ArgumentNullException).GetConstructor([typeof(string), typeof(string)])!;
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Throw);
        }

        public static void FlexibleConvertBoxedFromHeadOfStack(this ILGenerator il, Type from, Type to)
        {
            MethodInfo? op;
            if (from == to)
            {
                il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
            }
            else if ((op = ReflectionUtils.GetConversionMethodInfo(from, to)) != null)
            {
                // this is handy for things like decimal <===> double
                il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][data-typed-value]
                il.Emit(OpCodes.Call, op); // stack is now [target][target][typed-value]
            }
            else
            {
                bool handled = false;
                OpCode opCode = default;
                switch (Type.GetTypeCode(from))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        handled = true;
                        switch (Type.GetTypeCode(to))
                        {
                            case TypeCode.Byte:
                                opCode = OpCodes.Conv_Ovf_I1_Un; break;
                            case TypeCode.SByte:
                                opCode = OpCodes.Conv_Ovf_I1; break;
                            case TypeCode.UInt16:
                                opCode = OpCodes.Conv_Ovf_I2_Un; break;
                            case TypeCode.Int16:
                                opCode = OpCodes.Conv_Ovf_I2; break;
                            case TypeCode.UInt32:
                                opCode = OpCodes.Conv_Ovf_I4_Un; break;
                            case TypeCode.Boolean: // boolean is basically an int, at least at this level
                            case TypeCode.Int32:
                                opCode = OpCodes.Conv_Ovf_I4; break;
                            case TypeCode.UInt64:
                                opCode = OpCodes.Conv_Ovf_I8_Un; break;
                            case TypeCode.Int64:
                                opCode = OpCodes.Conv_Ovf_I8; break;
                            case TypeCode.Single:
                                opCode = OpCodes.Conv_R4; break;
                            case TypeCode.Double:
                                opCode = OpCodes.Conv_R8; break;
                            default:
                                handled = false;
                                break;
                        }
                        break;
                }
                if (handled)
                {
                    il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][col-typed-value]
                    il.Emit(opCode); // stack is now [target][target][typed-value]
                    if (to == typeof(bool))
                    { // compare to zero; I checked "csc" - this is the trick it uses; nice
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldtoken, to); // stack is now [target][target][value][member-type-token]
                    il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!, null); // stack is now [target][target][value][member-type]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.InvariantCultureMethodInfo, null); // stack is now [target][target][value][member-type][culture]
                    il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type), typeof(IFormatProvider) })!, null); // stack is now [target][target][boxed-member-type-value]
                    il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
                }
            }
        }

        public static void EmitInt32(this ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        public static void EmitCastToReference(this ILGenerator il, Type type)
        {
            if (type.IsValueType)
                il.Emit(OpCodes.Unbox_Any, type);
            else
                il.Emit(OpCodes.Castclass, type);
        }

        public static void EmitLoadType(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Ldtoken, type); //[...][RuntimeHandle]
            il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);//[...][type]
        }

        /// <summary>
        /// 加载objectLocal，然后调用PropertyInfo中的GetMethod，得到PropertyValue
        /// </summary>
        /// <param name="il"></param>
        /// <param name="objectLocal"></param>
        /// <param name="propertyInfo"></param>
        public static void EmitGetPropertyValue(this ILGenerator il, LocalBuilder objectLocal, PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod()!;

            if (!getMethodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldloc, objectLocal);//[type-value]
            }

            if (propertyType.IsValueType || getMethodInfo.IsStatic)
            {
                il.EmitCall(OpCodes.Call, getMethodInfo, null); //[property-type-value]
            }
            else
            {
                il.EmitCall(OpCodes.Callvirt, getMethodInfo, null);//[property-object-value]
            }

            if (propertyType.IsValueType)
            {
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);//[property-object-value]
            }
        }

        public static void EmitSetPropertyValue(this ILGenerator il, LocalBuilder objectLocal, LocalBuilder propertyValueLocal, PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod(true)!;

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldloc, objectLocal);
            }

            il.Emit(OpCodes.Ldloc, propertyValueLocal);

            EmitCastToReference(il, propertyInfo.PropertyType);

            if (!setMethod.IsStatic && !propertyInfo.DeclaringType!.IsValueType)
            {
                il.EmitCall(OpCodes.Callvirt, setMethod, null);
            }
            else
            {
                il.EmitCall(OpCodes.Call, setMethod, null);
            }
        }
    }
}
