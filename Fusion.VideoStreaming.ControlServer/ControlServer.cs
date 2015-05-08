using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fusion.VideoStreaming
{
    class ControlServer : IDisposable
    {
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        ~ControlServer()
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
