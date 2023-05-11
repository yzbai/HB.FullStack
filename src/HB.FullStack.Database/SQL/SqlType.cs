
namespace HB.FullStack.Database.SQL
{
    internal enum SqlType
    {
        Select,

        Insert,

        UpdateUsingOldNewCompare,
        UpdateUsingTimestamp,
        UpdateWithoutConflictCheck,

        //Update,
        //UpdatePropertiesTimestamp,
        //UpdatePropertiesTimeless,
        //UpdateDeletedFields,

        //Delete,
        //DeleteByProperties,

        //AddOrUpdateModel
    }
}