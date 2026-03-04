using Microsoft.AspNetCore.Mvc;

namespace I_am_engineer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(ILogger<IdentityController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Login")]
        public string Get()
        {
            return "ok";
        }
    }
}
