using Duende.Bff;
using Microsoft.AspNetCore.Mvc;

namespace Host3
{
    [Route("local")]
    public class LocalApiController : ControllerBase
    {
        public IActionResult Get()
        {
            var data = new
            {
                Message = "Hello from local API",
                User = User.FindFirst("name")?.Value ?? User.FindFirst("sub").Value
            };

            return Ok(data);
        }
    }
}