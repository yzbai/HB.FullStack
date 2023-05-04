/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;


namespace HB.FullStack.Client.ApiClient
{
    internal class TokenResDeleteRequest : ApiRequest
    {
        public TokenResDeleteRequest() : base(nameof(TokenRes), ApiMethod.Delete, ApiRequestAuth.JWT, null)
        {
        }
    }
}