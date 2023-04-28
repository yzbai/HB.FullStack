using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// Mark this property to be record
    /// Can be applied on value types, string, Record, Immutable, any other class implements both INotifyPropertyChanging and INotifyPropertyChanged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TrackPropertyAttribute : Attribute
    {

    }
}
