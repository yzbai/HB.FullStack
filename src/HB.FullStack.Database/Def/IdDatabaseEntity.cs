namespace HB.FullStack.Database.Def
{
    public abstract class IdDatabaseEntity : DatabaseEntity
    {
        public abstract long Id { get; set; }
    }
}
