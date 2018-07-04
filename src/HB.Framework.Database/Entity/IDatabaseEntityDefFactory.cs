using System;

namespace HB.Framework.Database.Entity
{
    public interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef Get(Type domainType);
        DatabaseEntityDef Get<T>();
        int GetVarcharDefaultLength();
    }
}