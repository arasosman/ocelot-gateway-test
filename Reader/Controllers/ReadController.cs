using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Reader.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44317/write");

            var contentType = "text/plain";
            //httpRequest.ContentLength = info.Length;
            httpRequest.ContentType = contentType;
            httpRequest.Accept = "application/json";
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = true;
            httpRequest.SendChunked = true;
            httpRequest.Timeout = int.MaxValue;
            httpRequest.ContinueTimeout = int.MaxValue;
            httpRequest.ReadWriteTimeout = int.MaxValue;
            var postStream = httpRequest.GetRequestStream();
            using (var stream = new FileStream("../stream.txt", FileMode.Open))
            {
                stream.CopyTo(postStream);
            }
            postStream.Close();
            HttpWebResponse speechResponse = (HttpWebResponse)httpRequest.GetResponse();
            return "UPLOADED";
        }
    }
}