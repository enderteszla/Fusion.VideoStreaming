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
    class Instance : IDisposable
    {
        private volatile dynamic VisualizationServer;
        private volatile StreamingServer StreamingServer;

        public Instance(Type VisualizatorName)
        {
            StreamingServer = new StreamingServer();
            VisualizationServer = Activator.CreateInstance(VisualizatorName);
        }

        public bool prepare(Keys key, Vector2 mousePosition)
        {
            return DevCon.Prepare(VisualizationServer, @"..\Content\Content.xml", "Content");
        }
        public void start(Keys key, Vector2 mousePosition)
        {
            (new Thread(() =>
            {
                StreamingServer.Start();
            })).Start();
            VisualizationServer.Run(new string[0]);
        }
        public void keyUp(Keys key, Vector2 mousePosition)
        {
            VisualizationServer.keyUp(key, mousePosition);
        }
        public void keyDown(Keys key, Vector2 mousePosition)
        {
            VisualizationServer.keyDown(key, mousePosition);
        }
        public void stop(Keys key, Vector2 mousePosition)
        {
            StreamingServer.Stop();
            VisualizationServer.Exit();
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
            VisualizationServer.Dispose();
            StreamingServer.Dispose();
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