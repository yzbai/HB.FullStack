using System;
using System.Collections.Generic;
using System.Text.Json;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

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

    public class ModelController<TModel> : BaseController where TModel : Model
    {
        protected DbModelDef ModelDef { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public ModelController(IDbModelDefFactory modelDefFactory)
        {
            ModelDefFactory = modelDefFactory;

            ModelDef = ModelDefFactory.GetDef<TModel>()!;


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

        protected ChangedPack2 ConvertToChangedPack(ChangedPackDto cpt)
        {
            //Id
            ChangedPack2 cp = new ChangedPack2 { Id = cpt.Id };

            //ChangedProperties
            foreach (ChangedPropertyDto propertyDto in cpt.ChangedProperties)
            {
                DbModelPropertyDef? propertyDef = ModelDef.GetPropertyDef(propertyDto.PropertyName);

                if (propertyDef == null)
                {
                    throw WebApiExceptions.ChangedPropertyPackError($"包含不属于当前DbModel的属性:{propertyDto.PropertyName}", cpt, ModelDef.ModelFullName);
                }

                cp.ChangedProperties.Add(
                    new ChangedProperty2(
                        propertyDto.PropertyName,
                        SerializeUtil.FromJsonElement(propertyDef.Type, propertyDto.OldValue),
                        SerializeUtil.FromJsonElement(propertyDef.Type, propertyDto.NewValue)));
            }

            //AddtionalProperties
            foreach (KeyValuePair<string, JsonElement> addtionalDto in cpt.AddtionalProperties)
            {
                DbModelPropertyDef? propertyDef = ModelDef.GetPropertyDef(addtionalDto.Key);

                if (propertyDef == null)
                {
                    throw WebApiExceptions.ChangedPropertyPackError($"包含不属于当前DbModel的属性:{addtionalDto.Key}", cpt, ModelDef.ModelFullName);
                }

                cp.AddAddtionalProperty(
                    addtionalDto.Key,
                    SerializeUtil.FromJsonElement(propertyDef.Type, addtionalDto.Value));
            }

            return cp;
        }
    }
}
