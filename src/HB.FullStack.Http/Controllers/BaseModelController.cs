using System;
using System.Collections.Generic;
using System.Text.Json;

using HB.FullStack.Common;

using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;
using HB.FullStack.KVStore.KVStoreModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Web.Controllers
{
    public class BaseModelController<TModel> : BaseController where TModel : IModel
    {
        protected ModelDef ModelDef { get; }

        public IModelDefFactory ModelDefFactory { get; }

        public BaseModelController()
        {
            ModelDefFactory = GlobalWebApplicationAccessor.Services.GetRequiredService<IModelDefFactory>();

            Type modelType = typeof(TModel);

            if (typeof(DbModel).IsAssignableFrom(modelType))
            {
                ModelDef = ModelDefFactory.GetDef(modelType, ModelKind.Db);
            }
            else if (typeof(KVStoreModel).IsAssignableFrom(modelType))
            {
                ModelDef = ModelDefFactory.GetDef(modelType, ModelKind.KV);
            }
            else
            {
                ModelDef = ModelDefFactory.GetDef(modelType, ModelKind.UnKown);
            }
        }

        //[HttpGet]
        //public virtual IActionResult Get(
        //    [FromQuery] string[]? ids,
        //    [FromQuery] int? page,
        //    [FromQuery] int? perPage,
        //    [FromQuery] string? orderBys,
        //    [FromQuery] string[]? resIncludes,
        //    [FromQuery] string[]? wherePropertyNames,
        //    [FromQuery] string?[]? wherePropertyValues
        //    //[FromQuery] string?[]? whereOperator
        //    )
        //{
        //    //settings: allowId? allowIds? allow page? maxPerPage? allowIncludes? includesWhat? allowPropertyFilter?

        //    //deal with ids & orderBys & includes & propertyFilters

        //    //deal with page & perPage & orderBys & inclues & propertyFilters

        //    return Ok();
        //}
    }
}
