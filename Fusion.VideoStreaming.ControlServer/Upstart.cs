using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Fusion.VideoStreaming
{   
    public class Upstart
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            /*appBuilder.Use(async (env, next) =>
            {
                Console.WriteLine(string.Concat("Http method: ", env.Request.Method, ", path: ", env.Request.Path));
                await next();
                Console.WriteLine(string.Concat("Response code: ", env.Response.StatusCode));
            });*/

            // Is request a signal?
            RunWebApiConfiguration(appBuilder);

            // Or is it a static file request?
            appBuilder.UseStaticFiles(new StaticFileOptions() { FileSystem = new PhysicalFileSystem(@".\Content") });

            // Or simply / ?
            appBuilder.UseIndexPage();
        }
        private void RunWebApiConfiguration(IAppBuilder appBuilder) {
            HttpConfiguration httpConfiguration = new HttpConfiguration();
            httpConfiguration.Routes.MapHttpRoute(
                name: "Signal",
                routeTemplate: "signal/{action}/{Key}/{MouseX}/{MouseY}",
                defaults: new { controller = "Signal", Key = RouteParameter.Optional, MouseX = RouteParameter.Optional, MouseY = RouteParameter.Optional }
                );
            httpConfiguration.Formatters.Remove(httpConfiguration.Formatters.XmlFormatter);
            Console.WriteLine("Configuration done.");
            appBuilder.UseWebApi(httpConfiguration);
        }
    }
}
