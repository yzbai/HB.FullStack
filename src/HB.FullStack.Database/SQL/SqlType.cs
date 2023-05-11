
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
        //UpdatePropertiesTimestamp,
        //UpdatePropertiesTimeless,
        //UpdateDeletedFields,

        //Delete,
        //DeleteByProperties,

        //AddOrUpdateModel
    }
}