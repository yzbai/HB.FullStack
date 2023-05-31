using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class SmsValidationCodeRes : ValidatableObject, ISharedResource
    {
        public int Length { get; set; }

        public long? ExpiredAt {get;set;}

        public ModelKind GetKind() => ModelKind.Shared;

    }
}