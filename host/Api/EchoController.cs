using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Api
{
    [AllowAnonymous]
    public class EchoController : ControllerBase
    {
        [HttpGet("{**catch-all}")]
        public IActionResult Get()
        {
            string message;
            var sub = User.FindFirst(("sub"));
            
            if (!User.Identity.IsAuthenticated)
            {
                message = "Hello, anonymous caller";
            }
            else if (sub != null)
            {
                var userName = User.FindFirst(("name"));
                message = $"Hello user, {userName.Value}";
            }
            else
            {
                var client = User.FindFirst("client_id");
                message = $"Hello client, {client.Value}";
            }
            
            var response = new
            {
                path = Request.Path.Value,
                message = message,
                time = DateTime.UtcNow.ToString()
            };

            return Ok(response);
        }
    }
}
