using System.Web;

namespace ololo
{
    public class HelloWorldHandler : IHttpHandler
    {
        public HelloWorldHandler()
        {
        }
        public void ProcessRequest(HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;
            // This handler is called whenever a file ending 
            // in .sample is requested. A file with that extension
            // does not need to exist.
            Response.Write("<html>");
            Response.Write("<body>");
            Response.Write("<h1>Hello from a synchronous custom HTTP handler.</h1>");
            Response.Write("</body>");
            Response.Write("</html>");
        }
        public bool IsReusable
        {
            // To enable pooling, return true here.
            // This keeps the handler in memory.
            get { return false; }
        }
    }
}