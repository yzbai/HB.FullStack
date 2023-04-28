using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// Addtional Property always been recorded
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AddtionalPropertyAttribute : Attribute
    {

    }
}
