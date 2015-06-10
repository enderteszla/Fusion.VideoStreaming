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
            int id = ControlServer.AddInstance("GraphVis.Game,GraphVis");
            Instance Instance = ControlServer.GetInstance(id);
            if (Instance.Prepare())
            {
                Instance.Init();
                Instance.Start();
                Console.ReadLine();
                ControlServer.CleanUp();
            }
        }
    }
}