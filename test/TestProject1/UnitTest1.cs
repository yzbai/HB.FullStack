using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;

using HB.FullStack.Common.Files;

namespace TestProject1
{
    public enum UserLevel
    {
        UnRegistered = 0,
        Normal = 1,
        Vip = 2,
        //平民，骑士，大臣，王者
    }

    public static class DirectorySettings
    {
        

        /// <summary>
        /// 有哪些目录权限
        /// </summary>
        public static class Permissions
        {
            public static readonly DirectoryPermission PUBLIC = new DirectoryPermission
            {
                PermissionName = nameof(PUBLIC),
                TopDirectory = "public",
                //Regex = "^public[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.UnRegistered), nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.UnRegistered), nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1),
            };

            public static readonly DirectoryPermission CUSTOMER = new DirectoryPermission
            {
                PermissionName = nameof(CUSTOMER),
                TopDirectory = "customer",
                //////Regex = "^customer[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1)
            };

            public static readonly DirectoryPermission CUSTOMERPRIVATE = new DirectoryPermission
            {
                PermissionName = nameof(CUSTOMERPRIVATE),
                TopDirectory = "customerprivate/{USER_ID_PLACE_HOLDER}",
                //Regex = "^customerprivate[/\\]{USER_ID_PLACE_HOLDER}[/\\].*$",
                ReadUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                WriteUserLevels = new List<string> { nameof(UserLevel.Normal), nameof(UserLevel.Vip) },
                ExpiryTime = TimeSpan.FromHours(1),
                ContainsPlaceHoder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}",
                IsUserPrivate = true
            };

            public static IList<DirectoryPermission> All { get; } = new List<DirectoryPermission>
            {
                PUBLIC,
                CUSTOMER,
                CUSTOMERPRIVATE
            };
        }

        /// <summary>
        /// 有哪些具体的目录，分别需要使用哪个权限
        /// </summary>
        public static class Descriptions
        {
            //TODO: 修改这些ExpiryTime
            public static readonly DirectoryDescription PUBLIC = new DirectoryDescription
            {
                DirectoryName = nameof(PUBLIC),
                DirectoryPath = "public",
                DirectoryPermissionName = Permissions.PUBLIC.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription PUBLIC_AVATAR = new DirectoryDescription
            {
                DirectoryName = nameof(PUBLIC_AVATAR),
                DirectoryPath = "public" + Path.DirectorySeparatorChar + "avator",
                DirectoryPermissionName = Permissions.PUBLIC.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription CUSTOMER = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMER),
                DirectoryPath = "customer",
                DirectoryPermissionName = Permissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription CUSTOMER_TEMP = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMER_TEMP),
                DirectoryPath = "customer" + Path.DirectorySeparatorChar + "temp",
                DirectoryPermissionName = Permissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(10)
            };
            public static readonly DirectoryDescription SYSTEM = new DirectoryDescription
            {
                DirectoryName = nameof(SYSTEM),
                DirectoryPath = "system",
                DirectoryPermissionName = Permissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription SYSTEM_THEME = new DirectoryDescription
            {
                DirectoryName = nameof(SYSTEM_THEME),
                DirectoryPath = "system" + Path.DirectorySeparatorChar + "theme",
                DirectoryPermissionName = Permissions.CUSTOMER.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1)
            };
            public static readonly DirectoryDescription CUSTOMERPRIVATE = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMERPRIVATE),
                DirectoryPath = "customerprivate" + Path.DirectorySeparatorChar + "{USER_ID_PLACE_HOLDER}",
                DirectoryPermissionName = Permissions.CUSTOMERPRIVATE.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1),
                IsPathContainsPlaceHolder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}"
            };
            public static readonly DirectoryDescription CUSTOMERPRIVATE_TEMP = new DirectoryDescription
            {
                DirectoryName = nameof(CUSTOMERPRIVATE_TEMP),
                DirectoryPath = "customerprivate" + Path.DirectorySeparatorChar + "{USER_ID_PLACE_HOLDER}" + Path.DirectorySeparatorChar + "temp",
                DirectoryPermissionName = Permissions.CUSTOMERPRIVATE.PermissionName,
                ExpiryTime = TimeSpan.FromMinutes(1),
                IsPathContainsPlaceHolder = true,
                PlaceHolderName = "{USER_ID_PLACE_HOLDER}"
            };

            public static readonly IList<DirectoryDescription> All = new List<DirectoryDescription>
            {
                PUBLIC,
                PUBLIC_AVATAR,

                CUSTOMER,
                CUSTOMER_TEMP,

                SYSTEM,
                SYSTEM_THEME,

                CUSTOMERPRIVATE,
                CUSTOMERPRIVATE_TEMP
            };
        }

        public static class Directories
        {
            public static readonly Directory2 PUBLIC = Descriptions.PUBLIC.ToDirectory(null);
            public static readonly Directory2 PUBLIC_AVATAR = Descriptions.PUBLIC_AVATAR.ToDirectory(null);
            public static readonly Directory2 CUSTOMER = Descriptions.CUSTOMER.ToDirectory(null);
            public static readonly Directory2 CUSTOMER_TEMP = Descriptions.CUSTOMER_TEMP.ToDirectory(null);
            public static readonly Directory2 SYSTEM = Descriptions.SYSTEM.ToDirectory(null);
            public static readonly Directory2 SYSTEM_THEME = Descriptions.SYSTEM_THEME.ToDirectory(null);
            public static Directory2 CUSTOMERPRIVATE(Guid? userId) => Descriptions.CUSTOMERPRIVATE.ToDirectory(userId?.ToString());
            public static Directory2 CUSTOMERPRIVATE_TEMP(Guid? userId) => Descriptions.CUSTOMERPRIVATE_TEMP.ToDirectory(userId?.ToString());

        }
    }
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void Test_Temp()
        {
            string json1 = SerializeUtil.ToJson(DirectorySettings.Permissions.All);
            string json2 = SerializeUtil.ToJson(DirectorySettings.Descriptions.All);


        }


        [TestMethod]
        public void Test_ConnectionString()
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            var item = new TestGeneric<UnitTest1>();

            var types = item.GetType().GenericTypeArguments;

        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian()
        {

            IList<A> list = new List<A>();

            Assert.IsTrue(list is IEnumerable);

        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian2()
        {

            IList<A> list = new List<A>();

            Assert.IsTrue(list is IEnumerable<BaseA>);
        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian3()
        {

            IList<B> list = new List<B>();

            Assert.IsTrue(list is IEnumerable<object> e && !e.Any());

            IEnumerable er = list;

        }

        [TestMethod]
        public void TestConverter()
        {
            var guidConvertor = TypeDescriptor.GetConverter(typeof(Guid));

            Guid guid = Guid.NewGuid();

            string? str = guidConvertor.ConvertToString(guid);

            Guid? guid2 = (Guid?)guidConvertor.ConvertFromString(str!);

            Assert.AreEqual(guid, guid2!.Value);
        }

    }

    public class BaseA
    {

    }

    public class A : BaseA
    {
        public sealed override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class AA : A
    {
        //Can not override GetHashCode
    }

    public class AAA : AA
    {
        //Can not override GetHashCode
    }

    public class B
    {

    }

    public class TestGeneric<T>
    {
        public TestGeneric()
        {
            Console.WriteLine(nameof(T));
        }
    }
}