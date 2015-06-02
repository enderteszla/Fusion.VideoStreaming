using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationFunction = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fusion.VideoStreaming
{
    class IndexPage
    {
        public IndexPage(ApplicationFunction appFunc)
        {
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            Stream responseBody = environment["owin.ResponseBody"] as Stream;
            using (StreamWriter responseWriter = new StreamWriter(responseBody))
            {
                StreamReader r = new StreamReader(@"..\Content\index.html");
                string fileContent = r.ReadToEnd();
                r.Close();
                return responseWriter.WriteAsync(fileContent);
            }
        }
    }
}
