using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Writer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WriteController : ControllerBase
    {
        [HttpPost]
        public string Post()
        {
            using (var stream = new FileStream("../write.txt", FileMode.Create))
            {
                Request.Body.CopyToAsync(stream).Wait();
            }

            return "COPIED";
        }
        
        [HttpGet]
        public string Get()
        {
            return "osman";
        }
    }
}