using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin.Hosting;

namespace Fusion.VideoStreaming
{
    class WebLauncher
    {
        static void Main(string[] args)
        {
            StartOptions options = new StartOptions();
            // For successful use of the command below execute this with admin rights: netsh http add urlacl url=http://*:9000/ user=<username>
            options.Urls.Add("http://*:9000");
            using (WebApp.Start<Upstart>(options))
            {
                Console.WriteLine("Web server on http://194.85.163.237:19000 started.");
                Console.ReadLine();
                Facade.CleanUp();
                Console.WriteLine("Web server on http://194.85.163.237:19000 stopped.");
            }
        }
    }
}
