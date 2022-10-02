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

        protected BadRequestObjectResult Error(ErrorCode errorCode, string description)
        {
            return BadRequest(new ErrorCode(errorCode.Code, description));
        }
    }

    public class ModelController<TModel> : BaseController where TModel : IModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <param name="orderBys"></param>
        /// <param name="resIncludes"></param>
        /// <param name="wherePropertyNames"></param>
        /// <param name="wherePropertyValues"></param>
        /// <param name="whereOperator">names和values的关系：相等、小、大等等</param>
        /// <returns></returns>
        [HttpGet]
        public virtual IActionResult Get(
            [FromQuery] string[]? ids,
            [FromQuery] int? page,
            [FromQuery] int? perPage,
            [FromQuery] string? orderBys,
            [FromQuery] string[]? resIncludes,
            [FromQuery] string[]? wherePropertyNames,
            [FromQuery] string?[]? wherePropertyValues
            //[FromQuery] string?[]? whereOperator
            )
        {
            //settings: allowId? allowIds? allow page? maxPerPage? allowIncludes? includesWhat? allowPropertyFilter?

            //deal with ids & orderBys & includes & propertyFilters

            //deal with page & perPage & orderBys & inclues & propertyFilters

            return Ok();
        }
    }
}
