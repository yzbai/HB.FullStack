using System.Runtime.CompilerServices;

using HB.FullStack.Common.Models;

[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]

namespace HB.FullStack.Common
{

    public abstract class Model : ValidatableObject, IModel
    {
        //public virtual ModelKind GetKind()
        //{
        //    return ModelKind.Plain;
        //}
        public virtual ModelKind GetKind()
        {
            return ModelKind.Plain;
        }
    }
}