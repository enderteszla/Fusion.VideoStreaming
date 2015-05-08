using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Owin;

namespace Test
{   
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
        app.UseErrorPage();
#else
            app.UseWelcomePage("/123");
#endif
        }
    }
}
