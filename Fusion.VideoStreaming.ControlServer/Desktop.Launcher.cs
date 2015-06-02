using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    Console.ReadLine();
                    ControlServer.CleanUp();
                    return false;
            }
        }

        static void Main(string[] args)
        {
            // Some biolerplate to react to close window event
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            int id = ControlServer.AddInstance("GraphVis.Game,GraphVis");
            Instance Instance = ControlServer.GetInstance(id);
            // if (Instance.prepare())
            // {
                Instance.Init();
                Instance.Start();
            // }

            /* using (var Instance = new Instance(Type.GetType("GraphVis.Game,GraphVis")))
            {
                if (Instance.prepare())
                {
                    Instance.init();
                    Instance.start();
                }
            } */
        }
    }
}