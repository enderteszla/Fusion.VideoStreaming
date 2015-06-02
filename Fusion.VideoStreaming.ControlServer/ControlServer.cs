using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.VideoStreaming
{
    public static class ControlServer
    {
        public volatile static List<Instance> InstanceList;
        
        public static int AddInstance(string VisualizatorType) {
            int LastIndex;
            if (InstanceList == null)
            {
                InstanceList = new List<Instance>();
                LastIndex = 0;
            }
            else
            {
                LastIndex = InstanceList.IndexOf(InstanceList.Last());
            }
            Instance Instance = new Instance(Type.GetType(VisualizatorType));
            InstanceList.Add(Instance);
            return InstanceList.IndexOf(Instance);
        }

        public static Instance GetInstance(int id) {
            return InstanceList[id];
        }

        public static void CleanUp() {
            foreach(var Instance in InstanceList){
                Instance.Dispose();
            }
            InstanceList = null;
        }
    }
}
