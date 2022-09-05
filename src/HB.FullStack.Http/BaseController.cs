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
        public virtual IActionResult Get(string[]? ids, int? page, int? perPage, string? orderBys, string[]? resIncludes, PropertyFilter[]? propertyFilters)
        {
            //settings: allowId? allowIds? allow page? maxPerPage? allowIncludes? includesWhat? allowPropertyFilter?

            //deal with ids & orderBys & includes & propertyFilters

            //deal with page & perPage & orderBys & inclues & propertyFilters

            throw new NotImplementedException();
        }
    }
}
