using Duende.Bff;
using Microsoft.AspNetCore.Mvc;

namespace Host5
{
    [Route("local")]
    public class LocalApiController : ControllerBase
    {
        public IActionResult Get()
        {
            var data = new
            {
                Message = "Hello from local API"
            };

            return Ok(data);
        }
    }
}