﻿global using OnlyForJsonConstructorAttribute = System.Text.Json.Serialization.JsonConstructorAttribute;

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 包含构造一个Request的所有信息
    /// </summary>
    public abstract class ApiRequest : DTO
    {
        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        [JsonIgnore]
        [Required]
        public string ResName { get; set; } = null!;

        [JsonIgnore]
        public ApiMethod ApiMethod { get; set; }

        /// <summary>
        /// 如果没有指定，那么会使用ResBinding中指定的Auth
        /// </summary>
        [JsonIgnore]
        public ApiRequestAuth2? Auth { get; set; }

        [JsonIgnore]
        public string? Condition { get; set; }

        protected ApiRequest(string resName, ApiMethod apiMethod, ApiRequestAuth2? auth, string? condition)
        {
            ResName = resName;
            ApiMethod = apiMethod;
            Auth = auth;
            Condition = condition;
        }

        ///// <summary>
        ///// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        ///// </summary>
        //public sealed override int GetHashCode()
        //{
        //    return HashCode.Combine(GetChildHashCode(), ApiMethod, Auth, Condition, ResName);
        //}
    }
}