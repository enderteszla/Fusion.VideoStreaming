using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fusion.Input;
using Fusion.Mathematics;
using Fusion.VideoStreaming;

namespace Fusion.VideoStreaming
{
    class DesktopLauncher
    {
        static void Main(string[] args)
        {
            Assembly.LoadFrom("GraphVis.dll");
            int id = Facade.AddInstance("GraphVis.Game,GraphVis");
            Instance Instance = Facade.GetInstance(id);
            if (Instance.Prepare())
            {
                Instance.Init();
                Instance.Start();
                Console.ReadLine();
                Facade.CleanUp();
            }
        }
    }
}