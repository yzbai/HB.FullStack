using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Shared.SignInReceipt
{
    internal class SignInReceiptResDeleteRequest : ApiRequest
    {
        public SignInReceiptResDeleteRequest() : base(nameof(SignInReceiptRes), ApiMethod.Delete, ApiRequestAuth.JWT, null)
        {
        }
    }
}
