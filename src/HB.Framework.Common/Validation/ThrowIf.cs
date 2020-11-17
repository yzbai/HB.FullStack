#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HB.Framework.Common;
using HB.Framework.Common.Validate;
using Microsoft;

namespace System
{
    public static class ThrowIf
    {
        /// <summary>
        /// Null
        /// </summary>
        /// <param name="o"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        [return: NotNull]
        public static T Null<T>([ValidatedNotNull] T? o, string paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException(paramName);

            return o;
        }

        /// <summary>
        /// NotValid
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <exception cref="ValidateErrorException"></exception>
        [return: NotNull]
        public static T NotValid<T>(T o) where T : ValidatableObject
        {
            if (!o.IsValid())
            {
                throw new ValidateErrorException(o);
            }

            return o;
        }

        /// <summary>
        /// NotValid
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        [return: NotNull]
        public static IEnumerable<T> NotValid<T>([ValidatedNotNull] IEnumerable<T> ts) where T : ValidatableObject
        {
            if (ts.Any())
            {
                ts.ForEach(t =>
                {
                    if (!t.IsValid())
                    {
                        throw new ValidateErrorException(t);
                    }
                });
            }

            return ts;
        }

        /// <summary>
        /// NullOrEmpty
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static IDictionary<TKey, TValue> NullOrEmpty<TKey, TValue>([ValidatedNotNull] IDictionary<TKey, TValue>? dict, string paramName)
        {
            if (dict == null || !dict.Any())
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.DictionaryNullOrEmptyErrorMessage, paramName);
            }

            return dict;
        }

        /// <summary>
        /// NullOrEmpty
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static IEnumerable<T> NullOrEmpty<T>([ValidatedNotNull] IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || !lst.Any())
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.CollectionNullOrEmptyErrorMessage, paramName);
            }

            return lst;
        }

        /// <summary>
        /// Empty
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static IEnumerable<T> Empty<T>(IEnumerable<T> lst, string paramName)
        {
            if (!lst.Any())
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.CollectionNullOrEmptyErrorMessage, paramName);
            }

            return lst;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static string Empty(string str, string paramName)
        {
            if (str.Length == 0)
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.StringCanNotBeEmpty, paramName);
            }

            return str;
        }

        /// <summary>
        /// AnyNull
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static IEnumerable<T> AnyNull<T>([ValidatedNotNull] IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || lst.Any(t => t == null))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.CollectionAnyNullErrorMessage, paramName);
            }

            return lst;
        }

        /// <summary>
        /// NullOrEmpty
        /// </summary>
        /// <param name="o"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static string NullOrEmpty([ValidatedNotNull] string? o, string paramName)
        {
            if (string.IsNullOrEmpty(o))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.ParameterNullOrEmptyErrorMessage, paramName);
            }

            return o;
        }

        public static string? NotMobile([ValidatedNotNull] string? mobile, string paramName, bool canBeNull)
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
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.NotMobileErrorMessage, paramName);
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
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.NotPasswordErrorMessage, paramName);
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
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.NotLoginNameErrorMessage, paramName);
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
                    throw new ArgumentNullException(HB.Framework.Common.Properties.Resources.NotEmailErrorMessage, paramName);
                }
            }

            if (!ValidationMethods.IsEmail(email))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.NotEmailErrorMessage, paramName);
            }

            return email;
        }

        /// <summary>
        /// NotEqual
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: MaybeNull]
        public static string? NotEqual(string? a, string? b, string paramName)
        {
            if (a == null && b != null || a != null && !a.Equals(b, GlobalSettings.Comparison))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.StringNotEqualErrorMessage, paramName);
            }

            return a;
        }
    }

    public static class ThrowIfExtensions
    {
        /// <summary>
        /// ThrowIfNull
        /// </summary>
        /// <param name="o"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static T ThrowIfNull<T>([ValidatedNotNull] this T? o, string paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException(paramName);

            return o;
        }

        /// <summary>
        /// ThrowIfNullOrNotValid
        /// </summary>
        /// <param name="o"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static T ThrowIfNullOrNotValid<T>([ValidatedNotNull] this T? o, string paramName) where T : class, ISupportValidate
        {
            if (o == null)
                throw new ArgumentNullException(paramName);

            if (!o.IsValid())
            {
                throw new ValidateErrorException(o);
            }

            return o;
        }

        /// <summary>
        /// ThrowIfNullOrEmpty
        /// </summary>
        /// <param name="o"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static string ThrowIfNullOrEmpty([ValidatedNotNull] this string? o, string paramName)
        {
            if (string.IsNullOrEmpty(o))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.ParameterNullOrEmptyErrorMessage, paramName);
            }

            return o;
        }

        /// <summary>
        /// ThrowIfNullOrEmpty
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static IDictionary<TKey, TValue> ThrowIfNullOrEmpty<TKey, TValue>([ValidatedNotNull] this IDictionary<TKey, TValue>? dict, string paramName)
        {
            if (dict == null || !dict.Any())
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.DictionaryNullOrEmptyErrorMessage, paramName);
            }

            return dict;
        }

        /// <summary>
        /// ThrowIfNullOrEmpty
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: NotNull]
        public static IEnumerable<T> ThrowIfNullOrEmpty<T>([ValidatedNotNull] this IEnumerable<T>? lst, string paramName)
        {
            if (lst == null || !lst.Any())
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.CollectionNullOrEmptyErrorMessage, paramName);
            }

            return lst;
        }

        /// <summary>
        /// ThrowIfNotEqual
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        [return: MaybeNull]
        public static string? ThrowIfNotEqual(this string? a, string? b, string paramName)
        {
            if (a == null && b != null || a != null && !a.Equals(b, GlobalSettings.Comparison))
            {
                throw new ArgumentException(HB.Framework.Common.Properties.Resources.StringNotEqualErrorMessage, paramName);
            }

            return a;
        }
    }
}

#nullable restore