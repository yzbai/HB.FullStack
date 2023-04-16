using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class SignInReceiptResDeleteRequest : ApiRequest
    {
        public SignInReceiptResDeleteRequest() : base(nameof(SignInReceiptRes), ApiMethod.Delete, ApiRequestAuth.JWT, null)
        {
        }
    }
}
