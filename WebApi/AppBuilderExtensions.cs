using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace WebApi
{
    public static class AppBuilderExtensions
    {
        public static void UseIndexPage(this IAppBuilder appBuilder)
        {
            appBuilder.Use<IndexPage>();
        }
    }
}
