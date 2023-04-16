using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared.Resources
{
    public class SmsValidationCodeRes : ApiResource
    {
        public int Length { get; set; }
    }
}
