using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Input;
using Fusion.Mathematics;
using Fusion.VideoStreaming;
using Minesweeper;

namespace Fusion.VideoStreaming
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cs = new Instance<Minesweeper.Game>(args))
            {
                // if(cs.prepare(Keys.F2, Vector2.Zero)){
                cs.start(Keys.F2, Vector2.Zero);
                // }
            }
        }
    }
}