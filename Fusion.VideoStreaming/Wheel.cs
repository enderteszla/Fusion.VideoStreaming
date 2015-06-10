using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.VideoStreaming
{
    internal class Wheel : IDisposable
    {
        private String[] FileNames;
        private int c;

        public Wheel(int number, int StartIndex = 0)
        {
            FileNames = new String[number];
            String directory = Properties.Settings.Default.fakeImageDir;
            String name = Properties.Settings.Default.fakeImageName;
            String extension = Properties.Settings.Default.fakeImageExtension;
            for (var i = 0; i < FileNames.Length; i++)
            {
                FileNames[i] = String.Format(@"{0}{1}{2,3:000}.{3}", directory, name, i, extension);
            }
            c = StartIndex;
        }
        public void Set(int Index)
        {
            c = Index;
        }
        public Wheel Next()
        {
            c = (c < FileNames.Length - 1) ? c + 1 : 0;
            return this;
        }
        public String Current()
        {
            return FileNames[c];
        }

        public void Dispose() {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
