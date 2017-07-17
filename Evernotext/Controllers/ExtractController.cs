using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NReadability;
using System.Net;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace Evernotext.Controllers
{
    [Route("api/Extract")]
    public class ExtractController : Controller
    {
        private AppSettings _appSettings;

        public ExtractController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [Produces("text/html")]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string q, [FromQuery] string e, [FromQuery] string f)
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

                if (string.IsNullOrEmpty(f) || f != "y")
                    content = transcodedContent.ExtractedContent;

            var posHead = content.IndexOf("<head");
            if (posHead > 0)
            {
                var endHead = content.IndexOf('>', posHead)+1;
                content = content.Insert(endHead, string.Format("<base href='{0}' />", q));
            } // Fix relative path error

                if (!string.IsNullOrEmpty(e))
                    await SendMailAsync(e, transcodedContent.ExtractedTitle, content, q);

                return Ok(content);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task SendMailAsync(string e, string title, string body, string url="")
        {
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(e);
            myMessage.From = new EmailAddress(_appSettings.EmailFrom, _appSettings.NameEmailFrom);
            myMessage.Subject = title;


            myMessage.HtmlContent = body;
            if (!string.IsNullOrEmpty(url))
            {
                myMessage.Subject += " " + url;
                myMessage.HtmlContent += "<span>Link referencia: " + url + "</span>";
            }

            var transportWeb = new SendGrid.SendGridClient(_appSettings.SendGridKey);
            var response = await transportWeb.SendEmailAsync(myMessage);
        }
    }
}