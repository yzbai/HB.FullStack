using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.Components.HealthCheck
{
    public class ServerHealthGetRequest : ApiRequest
    {
        public ServerHealthGetRequest() : base(nameof(ServerHealthRes), ApiMethod.Get, ApiRequestAuth.NONE, null)
        {
        }
    }
}
