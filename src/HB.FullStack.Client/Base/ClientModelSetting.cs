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

namespace HB.FullStack.Client.Base
{
    public class ClientModelSetting
    {
        public TimeSpan ExpiryTime { get; set; }

        public bool AllowOfflineRead { get; set; }

        public bool AllowOfflineAdd { get; set; }

        public bool AllowOfflineUpdate { get; set; }

        public bool AllowOfflineDelete { get; set; }
    }
}