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
            int id = Facade.AddInstance("GraphVis.Game,GraphVis");
            Instance Instance = Facade.GetInstance(id);
            Instance.Init();

            return Request.CreateResponse<Response>(new Response { Endpoint = Endpoint(request), InstanceID = id });
        }

        [HttpPost]
        public HttpResponseMessage Start([FromUri] SignalParameters parameters)
        {
            // ... Start GameServer
            Facade.GetInstance(parameters.InstanceID).Start();

            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage KeyUp([FromUri] SignalParameters parameters)
        {
            // ... Handle signal
            Facade.GetInstance(parameters.InstanceID).KeyUp(parameters.Key,parameters.MouseX,parameters.MouseY);

            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage KeyDown([FromUri] SignalParameters parameters)
        {
            // ... Handle signal
            Facade.GetInstance(parameters.InstanceID).KeyDown(parameters.Key, parameters.MouseX, parameters.MouseY);

            return Request.CreateResponse<object>(new object());
        }

        [HttpPost]
        public HttpResponseMessage Stop([FromUri] SignalParameters parameters)
        {
            // ... Stop StreamingServer, stop GameServer
            Facade.GetInstance(parameters.InstanceID).Stop();

            return Request.CreateResponse<object>(new object());
        }

        private string Endpoint(HttpRequestMessage request) {
            string RemoteIP = (string)((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
            string IP;
            string Port;

            if (IsLocalIP(RemoteIP))
            {
                IP = "192.168.14.1";
                Port = "9001";
            }
            else if (IPAddress.IsLoopback(IPAddress.Parse(RemoteIP)))
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
            return String.Format("http://{0}:{1}/{2}", IP, Port, FileName);
        }

        private bool IsLocalIP(string IP){
            try
            {
                foreach (IPAddress HostIP in Dns.GetHostAddresses(IP))
                {
                    foreach (IPAddress LocalIP in Dns.GetHostAddresses(Dns.GetHostName()))
                    {
                        if (AreInTheSameNetwork(HostIP,"255.255.0.0",LocalIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private bool AreInTheSameNetwork(IPAddress IP0, string SubNet, IPAddress IP1)
        {
            int SubNetInt = BitConverter.ToInt32(IPAddress.Parse(SubNet).GetAddressBytes(), 0);
            int IP0Int = BitConverter.ToInt32(IP0.GetAddressBytes(), 0);
            int IP1Int = BitConverter.ToInt32(IP1.GetAddressBytes(), 0);
            int networkPortionofFirstIP = IP0Int & SubNetInt;
            int networkPortionofSecondIP = IP1Int & SubNetInt;
            if ((IP0Int & SubNetInt) == (IP1Int & SubNetInt))
            {
                return true;
            }
            return false;
        }
    }
}
