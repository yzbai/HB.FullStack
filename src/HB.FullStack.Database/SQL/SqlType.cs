
namespace HB.FullStack.Database.SQL
{
    internal enum SqlType
    {
        SelectModel,

        AddModel,

        UpdateModel,
        UpdateProperties,
        UpdatePropertiesUsingOldNewCompare,
        UpdateDeletedFields,

        Delete,
        DeleteByProperties,

        AddOrUpdateModel
    }
}