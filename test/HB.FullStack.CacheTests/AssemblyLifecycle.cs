﻿using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.BaseTest
{
    [TestClass]
    public class AssemblyLifecycle
    {
        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext _) => await BaseAssemblyInit(_);

        [AssemblyCleanup]
        public static void AssemblyCleanup() => BaseAssemblyCleanup();
    }
}