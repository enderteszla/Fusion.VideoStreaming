using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Development;
using Fusion.Input;
using Fusion.Mathematics;

namespace Fusion.VideoStreaming
{
    public class Instance : IDisposable
    {
        private volatile dynamic VisualizationServer;
        private volatile StreamingServer StreamingServer;

        public Instance(Type VisualizatorName)
        {
            StreamingServer = new StreamingServer();
            VisualizationServer = Activator.CreateInstance(VisualizatorName);
        }

        public bool Prepare()
        {
            return DevCon.Prepare(VisualizationServer, @"..\Content\Content.xml", "Content");
        }

        public void Init() {
            StreamingServer.Start();
        }

        public void Start()
        {
            // StreamingThread = new Thread(StreamingServer.Start);
            // StreamingThread.Start();
            VisualizationServer.Run(new string[0]);
        }
        public void KeyUp(string KeyCode,float MouseX,float MouseY)
        {
            Keys Key;
            if (Enum.TryParse<Keys>(KeyCode, out Key))
            {
                VisualizationServer.keyUp(Key, new Vector2(MouseX, MouseY));
            }
        }
        public void KeyDown(string KeyCode,float MouseX,float MouseY)
        {
            Keys Key;
            if (Enum.TryParse<Keys>(KeyCode, out Key))
            {
                VisualizationServer.keyDown(Key, new Vector2(MouseX, MouseY));
            }
        }
        public void Stop()
        {
            if (StreamingServer != null)
            {
                StreamingServer.Stop();
            }
            if (VisualizationServer != null)
            {
                VisualizationServer.Exit();
            }
        }

        /*public void fire(GameServerEventType Type, String Key, Vector2 coords = ) {
            Keys key;
            if (Enum.TryParse<Keys>(Key,out key)) {
                if (key == Keys.LeftButton || key == Keys.RightButton) {
                    // ...
                }
            }
        }*/
        public void Dispose()
        {
            Dispose(true);
            if (VisualizationServer != null)
            {
                VisualizationServer.Dispose();
            }
            if (StreamingServer != null)
            {
                StreamingServer.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~Instance()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}