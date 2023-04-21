/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.Components.User
{
    public class UserProfileResGetByUserIdRequest : ApiRequest
    {
        [NoEmptyGuid]
        [RequestQuery]
        public Guid UserId { get; set; }

        public UserProfileResGetByUserIdRequest(Guid userId) : base(nameof(UserProfileRes), ApiMethod.Get, ApiRequestAuth.JWT, SharedNames.Conditions.ByUserId)
        {
            UserId = userId;
        }
    }
}