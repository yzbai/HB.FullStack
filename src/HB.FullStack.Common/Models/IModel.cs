using System.Runtime.CompilerServices;

using HB.FullStack.Common.Models;

[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]
namespace HB.FullStack.Common
{
    public interface IModel : IValidatableObject
    {
        /// <summary>
        /// 故意不做成属性
        /// </summary>
        ModelKind GetKind();
    }
}