using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.VideoStreaming
{
    internal struct Wheel
    {
        private String[] FileNames;
        private int c;

        public Wheel(int Number,int StartIndex = 0)
        {
            FileNames = new String[Number];
            String Directory = Properties.Settings.Default.fakeImageDir;
            String Name = Properties.Settings.Default.fakeImageName;
            String Extension = Properties.Settings.Default.fakeImageExtension;
            for (var i = 0; i < FileNames.Length; i++)
            {
                FileNames[i] = String.Format(@"{0}{1}{2,3:000}.{3}", Directory, Name, i, Extension);
            }
            c = StartIndex;
        }
        public void Set(int Index) {
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
    }
}
