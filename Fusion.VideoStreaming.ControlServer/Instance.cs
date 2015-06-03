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
        private dynamic VisualizationServer;
        private StreamingServer StreamingServer;
        private DateTime StreamingStartInstance;

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
            StreamingStartInstance = StreamingServer.Start();
        }

        public void Start()
        {
            VisualizationServer.SetStartInstance(StreamingStartInstance);
            VisualizationServer.Run(new string[0]);
        }
        public void KeyUp(int Key,float MouseX,float MouseY)
        {
            try
            {
                VisualizationServer.KeyUp((Keys)Key, new Vector2(MouseX, MouseY));
            }
            catch { }
            if ((Keys)Key == Keys.Escape) {
                Dispose();
            }
        }
        public void KeyDown(int Key,float MouseX,float MouseY)
        {
            try
            {
                VisualizationServer.KeyDown((Keys)Key, new Vector2(MouseX, MouseY));
            }
            catch { }
        }
        public void Stop()
        {
            Dispose();
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
                VisualizationServer.Exit();
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