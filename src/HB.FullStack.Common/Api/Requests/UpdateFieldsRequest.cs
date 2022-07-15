using HB.FullStack.Common.Api.Requests;


namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource
    {
        [OnlyForJsonConstructor]
        protected UpdateFieldsRequest() { }

        protected UpdateFieldsRequest(ApiRequestAuth auth, string? condition) : base(ApiMethodName.Patch, auth, condition) { }
    }
}