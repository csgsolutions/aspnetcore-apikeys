using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class EchoController : Controller
    {
        // GET api/values
        [HttpGet("{value}")]
        public string Get(string value)
        {
            return value;
        }

    }
}
