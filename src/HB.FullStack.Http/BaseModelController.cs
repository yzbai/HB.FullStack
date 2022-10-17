using System;
using System.Collections.Generic;
using System.Text.Json;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;
using HB.FullStack.KVStore.KVStoreModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.WebApi
{
    public class BaseModelController<TModel> : BaseController where TModel : Model
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

        protected ChangedPack ConvertToChangedPack(ChangedPackDto cpt)
        {
            //Id
            ChangedPack cp = new ChangedPack { Id = cpt.Id };

            //ChangedProperties
            foreach (ChangedPropertyDto propertyDto in cpt.ChangedProperties)
            {
                ModelPropertyDef? propertyDef = ModelDef.GetPropertyDef(propertyDto.PropertyName);

                if (propertyDef == null)
                {
                    throw WebApiExceptions.ChangedPropertyPackError($"包含不属于当前Model的属性:{propertyDto.PropertyName}", cpt, ModelDef.ModelFullName);
                }

                cp.ChangedProperties.Add(
                    new ChangedProperty(
                        propertyDto.PropertyName,
                        SerializeUtil.FromJsonElement(propertyDef.Type, propertyDto.OldValue),
                        SerializeUtil.FromJsonElement(propertyDef.Type, propertyDto.NewValue)));
            }

            //AddtionalProperties
            foreach (KeyValuePair<string, JsonElement> addtionalDto in cpt.AddtionalProperties)
            {
                ModelPropertyDef? propertyDef = ModelDef.GetPropertyDef(addtionalDto.Key);

                if (propertyDef == null)
                {
                    throw WebApiExceptions.ChangedPropertyPackError($"包含不属于当前Model的属性:{addtionalDto.Key}", cpt, ModelDef.ModelFullName);
                }

                cp.AddAddtionalProperty(
                    addtionalDto.Key,
                    SerializeUtil.FromJsonElement(propertyDef.Type, addtionalDto.Value));
            }

            return cp;
        }
    }
}
