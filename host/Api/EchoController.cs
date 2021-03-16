using Microsoft.AspNetCore.Mvc;
using System;

namespace Api
{
    public class EchoController : ControllerBase
    {
        [HttpGet("{**catch-all}")]
        public IActionResult Get()
        {
            var sub = User.FindFirst(("sub"));
            if (sub == null) throw new Exception("sub is missing");

            var response = new
            {
                path = Request.Path.Value,
                message = $"Hello, {sub.Value}!",
                time = DateTime.UtcNow.ToString()
            };

            return Ok(response);
        }
    }
}
