using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public interface ISmsValidationCodeRes : ISharedResource
    {
        public int Length { get; set; }

    }
}