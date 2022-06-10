using HB.FullStack.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.CommonTests
{
    [TestClass]
    public class ValidationTest
    {
        [TestMethod]
        public void Embeded_ValidableObject_Test()
        {
            OutterCls outterCls = new OutterCls();

            Assert.IsFalse(outterCls.IsValid());

            outterCls.InnerCls = new InnerCls();

            Assert.IsTrue(outterCls.IsValid());
        }
    }

    public class OutterCls : ValidatableObject
    {
        [Required]
        public InnerCls? InnerCls { get; set; }
    }

    public class InnerCls : ValidatableObject
    {
        [Required]
        public InnernerCls? InnernerCls { get; set; }
    }

    public class InnernerCls : ValidatableObject
    {
        [Required]
        public string? Name { get; set; }
    }
}
