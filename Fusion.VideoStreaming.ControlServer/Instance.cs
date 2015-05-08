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
    class Instance<T> : IDisposable where T : GameServer, new()
    {
        private volatile T GameServer;
        private volatile StreamingServer StreamingServer;
        private string[] Args;

        public Instance(string[] args)
        {
            StreamingServer = new StreamingServer();
            GameServer = new T();
            Args = args;
        }

        public bool prepare(Keys key, Vector2 mousePosition)
        {
            return DevCon.Prepare(GameServer, @"..\..\..\Content\Content.xml", "Content");
        }
        public void start(Keys key, Vector2 mousePosition)
        {
            (new Thread(() =>
            {
                StreamingServer.Start();
            })).Start();
            GameServer.Run(Args);
        }
        public void keyUp(Keys key, Vector2 mousePosition)
        {
            GameServer.keyUp(key, mousePosition);
        }
        public void keyDown(Keys key, Vector2 mousePosition)
        {
            GameServer.keyDown(key, mousePosition);
        }
        public void stop(Keys key, Vector2 mousePosition)
        {
            StreamingServer.Stop();
            GameServer.Exit();
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
            GameServer.Dispose();
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