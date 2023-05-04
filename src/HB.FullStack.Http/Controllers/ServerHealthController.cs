using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class ServerHealthController : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            //TODO: 记得用缓存，锁，等等
            //TODO: 所有AllowAnonymous都要进行统一的安全管理
            return Ok(new ServerHealthRes { ServerHealthy = ServerHealthy.UP });
        }
    }
}
