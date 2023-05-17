using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HB.FullStack.Common;
using HB.FullStack.Common.Validate;

using Microsoft;

namespace System
{
    public static class ThrowIf
    {
        public static long NotLongId(long id, string paramName)
        {
            if (id > 0)
            {
                return id;
            }

            throw new ArgumentException($"不合格 long Id. Parameter:{paramName}");
        }

        [return: NotNull]
        public static T Null<T>([ValidatedNotNull][NotNull] T? o, string paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException($"Parameter:{paramName}");

            return o;
        }

        [return: NotNull]
        public static T NotValid<T>(T o, string paramName) where T : ValidatableObject
        {
            if (!o.IsValid())
            {
                throw new ArgumentException($"不合法的实例. Parameter:{paramName}");
            }

            return o;
        }

        [return: NotNull]
        public static IEnumerable<T> NotValid<T>([ValidatedNotNull] IEnumerable<T> ts, string paramName) where T : ValidatableObject
        {
            if (ts.Any())
            {
                foreach (var t in ts)
                {
                    if (!t.IsValid())
                    {
                        throw new ArgumentException($"不合法的实例集合. Parameter:{paramName}");
                    }
                }
            }

            return ts;
        }

        [return: NotNull]
        public static IEnumerable<T> NullOrEmpty<T>([ValidatedNotNull][NotNull] IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || !lst.Any())
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            if (!lst.Any())
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return lst;
        }

        [return: NotNull]
        public static ICollection NullOrEmpty<T>([ValidatedNotNull][NotNull] ICollection? lst, string paramName)
        {
            if (lst == null)
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            if (lst.Count == 0)
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return lst;
        }

        public static IEnumerable<T> Empty<T>(IEnumerable<T> lst, string paramName)
        {
            if (!lst.Any())
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            return lst;
        }

        public static void Empty(ref Guid guid, string paramName)
        {
            if (guid == Guid.Empty)
            {
                throw new ArgumentException($"Empty Guid. Parameter: {paramName}");
            }
        }

        public static string Empty(string str, string paramName)
        {
            if (str.Length == 0)
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return str;
        }

        [return: NotNull]
        public static IEnumerable<T> AnyNull<T>([ValidatedNotNull][NotNull] IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || lst.Any(t => t == null))
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            return lst;
        }

        [return: NotNull]
        public static string NullOrEmpty([ValidatedNotNull][NotNull] string? o, string paramName)
        {
            if (string.IsNullOrEmpty(o))
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

#pragma warning disable CS8777 // net standard 2.0 不能良好识别 string.IsNullOrEmpty
            return o!;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

        public static string? NotMobile(string? mobile, string paramName, bool canBeNull)
        {
            if (mobile == null)
            {
                if (canBeNull)
                {
                    return null;
                }
                else
                {
                    throw new ArgumentNullException(paramName);
                }
            }

            if (!ValidationMethods.IsMobilePhone(mobile))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return mobile;
        }

        public static string? NotPassword(string? password, string paramName, bool canBeNull)
        {
            if (canBeNull && password == null)
            {
                return password;
            }

            if (!ValidationMethods.IsPassword(password))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return password;
        }

        public static string? NotLoginName(string? loginName, string paramName, bool canBeNull)
        {
            if (loginName == null)
            {
                if (canBeNull)
                {
                    return null;
                }
                else
                {
                    throw new ArgumentNullException(paramName);
                }
            }

            if (!ValidationMethods.IsLoginName(loginName))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return loginName;
        }

        public static string? NotEmail(string? email, string paramName, bool canBeNull)
        {
            if (email == null)
            {
                if (canBeNull)
                {
                    return null;
                }
                else
                {
                    throw new ArgumentNullException($"Parameter:{paramName}");
                }
            }

            if (!ValidationMethods.IsEmail(email))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return email;
        }

        [return: MaybeNull]
        public static string? NotEqual(string? a, string? b, string paramName)
        {
            if (a == null && b != null || a != null && !a.Equals(b, Globals.Comparison))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return a;
        }

        public static void CountNotEqual<T1, T2>(IList<T1> a, IList<T2> b, string message)
        {
            if (a.Count != b.Count)
            {
                throw new ArgumentException(message);
            }
        }

        public static int NotEqual(int a, int b, string paramName1, string paramName2)
        {
            if (a != b)
            {
                throw new ArgumentException($"Parameter1:{paramName1} 不等于 Parameter2:{paramName2}");
            }

            return a;
        }
    }

    public static class ThrowIfExtensions
    {
        [return: NotNull]
        public static T ThrowIfNotValid<T>(this T o, string paramName) where T : ValidatableObject
        {
            if (!o.IsValid())
            {
                throw new ArgumentException($"不合法的实例. Parameter:{paramName}");
            }

            return o;
        }

        public static T ThrowIfNull<T>([ValidatedNotNull][NotNull] this T? o, string? paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException($"Parameter:{paramName}");

            return o;
        }

        public static T ThrowIfNullOrNotValid<T>([ValidatedNotNull][NotNull] this T? o, string paramName) where T : class, IValidatableObject
        {
            if (o == null)
                throw new ArgumentNullException($"Parameter:{paramName}");

            if (!o.IsValid())
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return o;
        }

        [return: NotNull]
        public static string ThrowIfNullOrEmpty([ValidatedNotNull][NotNull] this string? o, string paramName)
        {
            if (string.IsNullOrEmpty(o))
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

#pragma warning disable CS8777 // net standard 2.0 不能良好识别string.IsNullOrEmpty()
            return o!;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

        [return: NotNull]
        public static IDictionary<TKey, TValue> ThrowIfNullOrEmpty<TKey, TValue>([ValidatedNotNull][NotNull] this IDictionary<TKey, TValue>? dict, string paramName)
        {
            if (dict == null || !dict.Any())
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            return dict;
        }

        [return: NotNull]
        public static IEnumerable<T> ThrowIfNullOrEmpty<T>([ValidatedNotNull][NotNull] this IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || !lst.Any())
            {
                throw new ArgumentNullException($"Parameter:{paramName}");
            }

            return lst;
        }

        [return: MaybeNull]
        public static string? ThrowIfNotEqual(this string? a, string? b, string paramName)
        {
            if (a == null && b != null || a != null && !a.Equals(b, Globals.Comparison))
            {
                throw new ArgumentException($"Parameter:{paramName}");
            }

            return a;
        }
    }
}

