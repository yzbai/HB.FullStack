/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Client.MauiLib.Components;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.MauiLib.Startup
{
    public class MauiOptions : IOptions<MauiOptions>
    {
        public MauiOptions Value => this;

        public IList<IntroduceContent> IntoduceContents { get; set; } = new List<IntroduceContent>();
        public string? UrlOfPrivacyAgreement { get; set; }
        public string? UrlOfServiceAgreement { get; set; }
        public string DefaultAvatarFileName { get; set; } = null!;
    }
}