using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Input;
using Fusion.Mathematics;
using Fusion.VideoStreaming;
using GraphVis;

namespace Fusion.VideoStreaming
{
    class DesktopLauncher
    {
        static void Main(string[] args)
        {
            /* Instance.init(Type.GetType("GraphVis.Game,GraphVis"));
            if (Instance.prepare(Keys.F2, Vector2.Zero))
            {
                Instance.start(Keys.F2, Vector2.Zero);
            } */
            using (var cs = new Instance(Type.GetType("GraphVis.Game,GraphVis")))
            {
                if(cs.prepare(Keys.F2, Vector2.Zero)){
                    cs.start(Keys.F2, Vector2.Zero);
                }
            }
        }
    }
}