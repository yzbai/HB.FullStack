using HB.FullStack.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace HB.FullStack.CommonTests
{
    public class ValidationTest
    {
        [Fact]
        public void Embeded_ValidableObject_Test()
        {
            OutterCls outterCls = new OutterCls();

            Assert.False(outterCls.IsValid());

            outterCls.InnerCls = new InnerCls();

            Assert.True(outterCls.IsValid());
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
