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
        private dynamic Visualisator;
        private Type VisualisatorName;
        private Streamer Streamer;
        private DateTime StreamingStartInstance;

        public Instance(Type VisualisatorName)
        {
            Streamer = new Streamer();
            Visualisator = Activator.CreateInstance(VisualisatorName);
            this.VisualisatorName = VisualisatorName;
        }

        public bool Prepare()
        {
            return DevCon.Prepare(Visualisator, String.Format(@"..\Content\{0}\Content.xml", VisualisatorName.Namespace), "Content");
        }

        public void Init() {
            StreamingStartInstance = Streamer.Start();
        }

        public void Start()
        {
            Visualisator.SetStartInstance(StreamingStartInstance);
            Visualisator.Run(new string[0]);
        }
        public void KeyUp(int Key,float MouseX,float MouseY)
        {
            try
            {
                Visualisator.KeyUp((Keys)Key, new Vector2(MouseX, MouseY));
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
                Visualisator.KeyDown((Keys)Key, new Vector2(MouseX, MouseY));
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
            if (Visualisator != null)
            {
                Visualisator.Exit();
            }
            if (Streamer != null)
            {
                Streamer.Dispose();
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