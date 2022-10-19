using System;

namespace HB.FullStack.Common.Models
{
    public interface IModelDefProvider
    {
        ModelKind ModelKind { get; }

        ModelDef? GetModelDef(Type type);
    }
}
