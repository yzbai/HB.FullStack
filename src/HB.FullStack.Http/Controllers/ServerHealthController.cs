using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class ServerHealthController : BaseController
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new ServerHealthRes { ServerHealthy = ServerHealthy.UP });
        }
    }
}
