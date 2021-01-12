﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api
{
    public enum ApiErrorCode
    {
        //ApiUnkown,

        NoAuthority,
        AccessTokenExpired,

        ModelValidationError,
        ApiNotAvailable,
        ApiErrorWrongFormat,
        NotApiResourceEntity,
        ApiSmsCodeInvalid,
        ApiPublicResourceTokenNeeded,
        ApiPublicResourceTokenError,
        ApiUploadEmptyFile,
        ApiUploadOverSize,
        ApiUploadWrongType,
        ApiHttpsRequired,
        FromExceptionController,
        ApiCapthaError,
        ApiUploadFailed,

        ServerError,
        ClientError,
        NullReturn,
    }
}