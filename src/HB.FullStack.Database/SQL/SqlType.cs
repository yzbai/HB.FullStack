
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
        //UpdatePropertiesTimeless,
        //UpdateDeletedFields,

        //Delete,
        //DeleteByProperties,

        //AddOrUpdateModel
    }
}