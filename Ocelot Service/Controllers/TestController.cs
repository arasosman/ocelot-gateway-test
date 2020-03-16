using Microsoft.AspNetCore.Mvc;

namespace Ocelot_Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        // GET
        public string Index()
        {
            return "osman";
        }
    }
}