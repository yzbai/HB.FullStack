using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.ApiClient;

namespace HB.FullStack.Client.Components.HealthCheck
{

    public class HealthGetRequest : ApiRequest
    {
        public HealthGetRequest(string resName, ApiMethod apiMethod, ApiRequestAuth? auth, string? condition) : base(resName, apiMethod, auth, condition)
        {
        }
    }
}
