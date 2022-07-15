namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// One kind of Data Transfer Objects. Mainly using on net.
    /// </summary>
    public class ApiResource : ValidatableObject, IDTO
    {
        //[Range(0, int.MaxValue)]
        //public int Version { get; set; } = -1;

        //public string LastUser { get; set; } = string.Empty;

        //public DateTimeOffset LastTime { get; set; }

        //public abstract override int GetHashCode();
    }

    public class EmptyApiResource : ApiResource
    {
        public static EmptyApiResource Value { get; }

        static EmptyApiResource()
        {
            Value = new EmptyApiResource();
        }

        private EmptyApiResource() { }
    }

    //public abstract class GuidResource : ApiResource
    //{
    //    [NoEmptyGuid]
    //    public Guid Id { get; set; }

    //    public sealed override int GetHashCode()
    //    {
    //        return HashCode.Combine(GetChildHashCode(), Id, LastTime, Version, LastUser);
    //    }

    //    protected abstract int GetChildHashCode();
    //}

    //public abstract class LongIdResource : ApiResource
    //{
    //    [LongId2]
    //    public long Id { get; set; } = -1;

    //    public sealed override int GetHashCode()
    //    {
    //        return HashCode.Combine(GetChildHashCode(), Id, LastTime, Version, LastUser);
    //    }

    //    protected abstract int GetChildHashCode();
    //}
}
