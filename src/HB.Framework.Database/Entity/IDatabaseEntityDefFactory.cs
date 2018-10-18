using System;

namespace HB.Framework.Database.Entity
{
    public interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef GetDef(Type domainType);
        DatabaseEntityDef GetDef<T>();
        int GetVarcharDefaultLength();
    }
}