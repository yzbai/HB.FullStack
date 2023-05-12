
namespace HB.FullStack.Database.SQL
{
    internal enum SqlType
    {
        Select,

        Insert,

        UpdateIgnoreConflictCheck,
        UpdateUsingOldNewCompare,
        UpdateUsingTimestamp,

        //Update,
        UpdatePropertiesUsingTimestamp,
        UpdatePropertiesIgnoreConflictCheck,
        //UpdatePropertiesTimeless,
        //UpdateDeletedFields,

        //Delete,
        //DeleteByProperties,

        //AddOrUpdateModel
    }
}