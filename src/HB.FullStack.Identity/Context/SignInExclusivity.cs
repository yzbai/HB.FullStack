namespace HB.FullStack.Server.Identity.Context
{
    public enum SignInExclusivity
    {
        None,
        LogOffAllOthers,
        LogOffAllButWeb,
        LogOffSameIdiom,
    }
}