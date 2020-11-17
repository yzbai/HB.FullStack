namespace HB.Framework.Database.SQL
{
    /// <summary>
    /// Wrapper of the String
    /// </summary>
    internal class PartialSqlString
    {
        public PartialSqlString(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
