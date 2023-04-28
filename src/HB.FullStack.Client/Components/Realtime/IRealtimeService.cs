/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components
{
    public interface IRealtimeService
    {
        /// <summary>
        /// 一般在两个地方注册：
        /// 1， ViewModel中，实时反映ObservableObject变化
        /// 2， 系统，将变化写入数据库
        /// </summary>
        event Func<Task>? ServerCallTest;
    }
}