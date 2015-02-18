using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.VideoStreaming
{
	public class StreamingServer : IDisposable
	{
		private String ffmpegPath, frameRate, fakeImageDir, fakeImageName, fakeImageExtension, vlcPath, port, fakeVideoName, fakeVideoExtension, batchFileDir, batchFileName;
		private Process controlProcess;
		private GameServer gameServer;

		public StreamingServer(int frameRate = 0, int port = 0)
		{
			this.ffmpegPath = Properties.Settings.Default.ffmpegPath;
			this.frameRate = (frameRate == 0) ? Properties.Settings.Default.fps.ToString() : frameRate.ToString();
			this.fakeImageDir = Properties.Settings.Default.fakeImageDir;
			this.fakeImageName = Properties.Settings.Default.fakeImageName;
			this.fakeImageExtension = Properties.Settings.Default.fakeImageExtension;
			this.vlcPath = Properties.Settings.Default.vlcPath;
			this.port = (port == 0) ? Properties.Settings.Default.port.ToString() : port.ToString();
			this.fakeVideoName = Properties.Settings.Default.fakeVideoName;
			this.fakeVideoExtension = Properties.Settings.Default.fakeVideoExtension;
			this.batchFileDir = Properties.Settings.Default.batchFileDir;
			this.batchFileName = Properties.Settings.Default.batchFileName;
			Check();
		}

		private void Check()
		{
			if (!File.Exists(ffmpegPath))
			{
				throw new Exception(String.Format("File doesn't exist: {0}. Please provide correct ffmpeg path.",ffmpegPath));
			}
			if (!File.Exists(vlcPath))
			{
				throw new Exception(String.Format("File doesn't exist: {0}. Please provide correct vlc path.", vlcPath));
			}
			if (!Directory.Exists(fakeImageDir))
			{
				throw new Exception(String.Format("Directory doesn't exist: {0}. Please provide correct directory path to save fake images into.", fakeImageDir));
			}
			if (!Directory.Exists(batchFileDir))
			{
				throw new Exception(String.Format("Directory doesn't exist: {0}. Please provide correct directory path to save batch executable file into.", batchFileDir));
			}
		}

		public StreamingServer Assign(GameServer gameServer) {
			this.gameServer = gameServer;
			return this;
		}

		public void Start() {
			using (StreamWriter batchFile = new StreamWriter(String.Concat(batchFileDir,batchFileName), false))
			{
				String executionString = String.Format("\"{0}\" -loop 1 -framerate {1} -i \"{2}{3}%%03d.{4}\" -vcodec libx264 -r 30 -pix_fmt yuv420p -f mpegts - | \"{5}\" -I dummy --dummy-quiet - :sout=#transcode{{vcodec=theo,vb=800,acodec=vorb,ab=128,channels=2,samplerate=44100}}:http{{dst=:{6}/{7}.{8}}} :sout-keep vlc://quit", ffmpegPath, frameRate, fakeImageDir, fakeImageName, fakeImageExtension, vlcPath, port, fakeVideoName, fakeVideoExtension);
				batchFile.WriteLine(executionString);
				executionString = String.Format("del \"{0}{1}???.{2}\"", fakeImageDir, fakeImageName, fakeImageExtension);
				batchFile.WriteLine(executionString);
				executionString = String.Format("del \"{0}\"", String.Concat(batchFileDir, batchFileName));
				batchFile.WriteLine(executionString);
			}
			Thread.Sleep(1000);
			ProcessStartInfo controlProcessInfo = new ProcessStartInfo("cmd.exe", String.Format("/C {0}", String.Concat(batchFileDir, batchFileName)));
			controlProcess = Process.Start(controlProcessInfo);
		}

		public void Stop()
		{
			if(controlProcess != null)
			{
				controlProcess.CloseMainWindow();
			}
		}

		public void Dispose() {
			Dispose(true);
			Stop();
			GC.SuppressFinalize(this);
		}

		~StreamingServer() {
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
