using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HB.Framework.Services.ImageCode;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HB.Framework.AuthorizationServer.Web.Apis
{
    //TODO: 后期可以参照JWT server模式做一个 imagecode server
    [Route("api/[controller]")]
    public class ImageCodeController : Controller
    {
        private IImageCodeBiz _imageCodeBiz;

        public ImageCodeController(IImageCodeBiz imageCodeBiz)
        {
            _imageCodeBiz = imageCodeBiz;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            ImageCodeResult result = _imageCodeBiz.Generate(HttpContext.Session, 120, 30);

            return File(result.Image, result.ContentType);
        }
    }
}
