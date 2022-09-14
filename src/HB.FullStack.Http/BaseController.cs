using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.WebApi
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode)
        {
            return BadRequest(errorCode);
        }
    }

    public class ModelController<TModel> : BaseController where TModel : IModel
    {
        /// <summary>
        /// 通用Get
        /// </summary>
        [HttpGet]
        public virtual IActionResult Get(
            [FromQuery] string[]? ids,
            [FromQuery] int? page,
            [FromQuery] int? perPage,
            [FromQuery] string? orderBys,
            [FromQuery] string[]? resIncludes,
            [FromQuery] string[]? wherePropertyNames,
            [FromQuery] string?[]? wherePropertyValues)
        {
            //settings: allowId? allowIds? allow page? maxPerPage? allowIncludes? includesWhat? allowPropertyFilter?

            //deal with ids & orderBys & includes & propertyFilters

            //deal with page & perPage & orderBys & inclues & propertyFilters

            return Ok();
        }
    }
}
