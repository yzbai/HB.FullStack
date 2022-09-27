using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.ClientModels
{
    [PropertyTrackableObject]
    public partial class ClientDbModel : TimelessGuidDbModel
    {

        /// <summary>
        /// 改动时间，包括：
        /// 1. Update
        /// 2. Get from network
        /// </summary>
        public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;

    }
}
