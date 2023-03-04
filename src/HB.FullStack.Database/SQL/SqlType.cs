
namespace HB.FullStack.Database.SQL
{
    internal enum SqlType
    {
        SelectModel,

        AddModel,

        UpdateModel,
        UpdatePropertiesTimestamp,
        UpdatePropertiesTimeless,
        UpdateDeletedFields,

        Delete,
        DeleteByProperties,

        AddOrUpdateModel
    }
}