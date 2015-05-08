using System;
using System.Web;

namespace ololo
{
    class HandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context,
            string requestType, String url, String pathTranslated)
        {
            IHttpHandler handlerToReturn;
            if ("get" == context.Request.RequestType.ToLower())
            {
                handlerToReturn = new HelloWorldHandler();
            }
            else if ("post" == context.Request.RequestType.ToLower())
            {
                handlerToReturn = new HelloWorldAsyncHandler();
            }
            else
            {
                handlerToReturn = null;
            }
            return handlerToReturn;
        }
        public void ReleaseHandler(IHttpHandler handler)
        {
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}