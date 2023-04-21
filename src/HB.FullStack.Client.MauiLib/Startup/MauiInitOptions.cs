/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.MauiLib.Startup
{
    public class MauiInitOptions : IOptions<MauiInitOptions>
    {
        public MauiInitOptions Value => this;
    }
}