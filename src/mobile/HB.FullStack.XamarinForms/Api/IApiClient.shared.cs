using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;

namespace HB.FullStack.XamarinForms.Api
{
    public interface IApiClient
    {
        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;
        event AsyncEventHandler<object, ApiEventArgs> Responsed;

        JwtEndpointSetting GetDefaultJwtEndpointSetting();

        Task AddAsync<TRes>(AddRequest<TRes> request) where TRes : ApiResource2;
        Task AddAsync<TRes>(AddRequest<TRes> request, CancellationToken cancellationToken) where TRes : ApiResource2;

        Task UpdateAsync<TRes>(UpdateRequest<TRes> request) where TRes : ApiResource2;
        Task UpdateAsync<TRes>(UpdateRequest<TRes> request, CancellationToken cancellationToken) where TRes : ApiResource2;

        Task UpdateFieldsAsync<TRes>(UpdateFieldsRequest<TRes> request) where TRes : ApiResource2;
        Task UpdateFieldsAsync<TRes>(UpdateFieldsRequest<TRes> request, CancellationToken cancellationToken) where TRes : ApiResource2;

        Task DeleteAsync<TRes>(DeleteRequest<TRes> request) where TRes : ApiResource2;
        Task DeleteAsync<TRes>(DeleteRequest<TRes> request, CancellationToken cancellationToken) where TRes : ApiResource2;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;
        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TRes?> GetByIdAsync<TRes>(Guid id) where TRes : ApiResource2;
        Task<TRes?> GetByIdAsync<TRes>(Guid id, CancellationToken cancellationToken) where TRes : ApiResource2;

        Task<IEnumerable<TRes>> GetAllAsync<TRes>() where TRes : ApiResource2;
        Task<IEnumerable<TRes>> GetAllAsync<TRes>(CancellationToken cancellationToken) where TRes : ApiResource2;

        Task<Stream> GetStreamAsync(ApiRequest request);
        Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken);

        Task UploadAsync(UploadRequest request);
        Task UploadAsync(UploadRequest request, CancellationToken cancellationToken);
    }
}