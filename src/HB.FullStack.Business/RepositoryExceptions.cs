namespace System
{
    internal static class RepositoryExceptions
    {
        internal static Exception AddtionalPropertyNeeded(string modelFullName)
        {
            RepositoryException ex = new RepositoryException(ErrorCodes.ChangedPackError, "AddtionalPropertyNeeded");

            ex.Data["ModelName"] = modelFullName;

            return ex;
        }
    }
}