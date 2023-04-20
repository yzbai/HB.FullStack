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

using Microsoft.Extensions.Options;

namespace Todo.Client.MobileApp
{
    public class TodoAppOptions : IOptions<TodoAppOptions>
    {
        public TodoAppOptions Value => this;

        public static bool NeedLoginDefault { get; set; } = false;
    }
}