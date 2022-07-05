

namespace HB.FullStack.Database.SQL
{
    internal enum SqlType
    {
        AddEntity,
        UpdateEntity,
        UpdateFieldsUsingVersionCompare,
        UpdateFieldsUsingOldNewCompare,
        DeleteEntity,
        SelectEntity,
        Delete,
        AddOrUpdateEntity

    }
}