using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NReadability;
using System.Net;

namespace Evernotext.Controllers
{
    [Route("api/Extract")]
    public class ExtractController : Controller
    {
        [Produces("text/html")]
        [HttpGet]
        public IActionResult Get([FromQuery] string q)
        {
            var transcoder = new NReadabilityTranscoder();
            string content;

            if (string.IsNullOrEmpty(q))
                return NotFound();

            try
            {
                using (var wc = new WebClient())
                {
                    content = wc.DownloadString(q);
                }

                var transcodedContent =
                  transcoder.Transcode(new TranscodingInput(content));

                return Ok(transcodedContent.ExtractedContent);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        } 
    }
}