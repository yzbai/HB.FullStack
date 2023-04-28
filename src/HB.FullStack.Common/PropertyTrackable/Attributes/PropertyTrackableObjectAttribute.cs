using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// for source generation. Put IPropertyTrackableObject Implements into class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PropertyTrackableObjectAttribute : Attribute
    {

    }
}
