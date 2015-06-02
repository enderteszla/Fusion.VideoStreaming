using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Fusion.VideoStreaming
{
    public class SignalController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Prepare()
        {
            // ... Start StreamingServer (empty images + start VideoStreaming)
            string IP = "194.85.163.237";
            string Port = "19001";
            string FileName = "webcam.ogg";
            return Request.CreateResponse<String>(String.Format("http://{0}:{1}/{2}", IP, Port, FileName));
        }

        [HttpPost]
        public HttpResponseMessage Start()
        {
            // ... Start GameServer
            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage KeyUp([FromUri] SignalParameters parameters)
        {
            string Key = parameters.Key;
            float MouseX = parameters.MouseX;
            float MouseY = parameters.MouseY;
            // ... Handle signal
            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage KeyDown([FromUri] SignalParameters parameters)
        {
            string Key = parameters.Key;
            float MouseX = parameters.MouseX;
            float MouseY = parameters.MouseY;
            // ... Handle signal
            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage Stop()
        {
            // ... Stop StreamingServer, stop GameServer
            return Request.CreateResponse<object>(new object());
        }
    }
}
