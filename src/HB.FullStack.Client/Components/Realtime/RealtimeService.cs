/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components
{
    public class RealtimeService : IRealtimeService
    {
#pragma warning disable CS0067 // The event 'RealtimeService.ServerCallTest' is never used

        public event Func<Task>? ServerCallTest;

#pragma warning restore CS0067 // The event 'RealtimeService.ServerCallTest' is never used
    }
}