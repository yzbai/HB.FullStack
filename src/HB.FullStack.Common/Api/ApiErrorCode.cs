using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api
{
    public enum ApiErrorCode
    {
        ModelValidationError,
        ApiUnkown,
        ApiNotAvailable,
        ApiErrorWrongFormat,
        NotApiResourceEntity
    }
}
