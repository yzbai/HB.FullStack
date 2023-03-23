using System;

namespace HB.FullStack.Common.Models
{
    public interface IModelDefFactory
    {
        ModelDef GetDef<T>(ModelKind modelKind) where T : IModel => GetDef(typeof(T), modelKind);

        ModelDef GetDef(Type type, ModelKind modelKind);
    }
}
