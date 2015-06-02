using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;

namespace Fusion.VideoStreaming
{
    public class SignalController : ApiController
    {
        struct Response {
            public string Endpoint;
            public int InstanceID;
        }

        [HttpPost]
        public HttpResponseMessage Prepare(HttpRequestMessage request)
        {
            // ... Start StreamingServer (empty images + start VideoStreaming)
            int id = ControlServer.AddInstance("GraphVis.Game,GraphVis");
            Instance Instance = ControlServer.GetInstance(id);
            Instance.Init();

            string RemoteIP = (string)((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
            string IP;
            string Port;
            if ((new HashSet<string> { "192.168.0.100", "192.168.255.255" }).Contains(RemoteIP)) {
                IP = "192.168.14.1";
                Port = "9001";
            }
            else if (RemoteIP == "127.0.0.1")
            {
                IP = "127.0.0.1";
                Port = "9001";
            }
            else
            {
                IP = "194.85.163.237";
                Port = "19001";
            }
            string FileName = "webcam.ogg";

            return Request.CreateResponse<Response>(new Response { Endpoint = String.Format("http://{0}:{1}/{2}", IP, Port, FileName), InstanceID = id });
        }

        // [HttpPost]
        [HttpGet]
        public HttpResponseMessage Start([FromUri] SignalParameters parameters)
        {
            // ... Start GameServer
            ControlServer.GetInstance(parameters.InstanceID).Start();

            return Request.CreateResponse<object>(new object());
        }

        // [HttpPost]
        [HttpGet]
        public HttpResponseMessage KeyUp([FromUri] SignalParameters parameters)
        {
            // ... Handle signal
            ControlServer.GetInstance(parameters.InstanceID).KeyUp(parameters.Key,parameters.MouseX,parameters.MouseY);

            return Request.CreateResponse<object>(new object());
        }

        // [HttpPost]
        [HttpGet]
        public HttpResponseMessage KeyDown([FromUri] SignalParameters parameters)
        {
            // ... Handle signal
            ControlServer.GetInstance(parameters.InstanceID).KeyDown(parameters.Key, parameters.MouseX, parameters.MouseY);

            return Request.CreateResponse<object>(new object());
        }

        // [HttpPost]
        [HttpGet]
        public HttpResponseMessage Stop([FromUri] SignalParameters parameters)
        {
            // ... Stop StreamingServer, stop GameServer
            ControlServer.GetInstance(parameters.InstanceID).Stop();

            return Request.CreateResponse<object>(new object());
        }
    }
}
