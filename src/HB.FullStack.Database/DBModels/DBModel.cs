using HB.FullStack.Common;
using HB.FullStack.Common.Models;

/*
 * 
 */
namespace HB.FullStack.Database.DbModels
{
    public abstract class DbModel : ValidatableObject, IModel
    {
        /// <summary>
        /// 不是真正的删除，而是用Deleted=1表示删除。
        /// </summary>
        public bool Deleted { get; /*internal*/ set; }

        public string LastUser { get; set; } = string.Empty;

        public ModelKind GetKind() => ModelKind.Db;
    }
}