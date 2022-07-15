

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]

namespace HB.FullStack.Common
{
    public interface IModel : IValidatableObject { }

    public class Model : ValidatableObject, IModel { }

    public interface IDTO : IValidatableObject { }

    public interface IGuidIdModel
    {
        Guid Id { get; set; }
    }

    public interface ILongIdModel
    {
        long Id { get; set; }
    }

    public interface ITimestampModel : IModel
    {
        //int Version { get; set; }
        long Timestamp { get; set; }
    }
}