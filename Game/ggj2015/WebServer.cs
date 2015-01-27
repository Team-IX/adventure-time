using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ggj2015
{
	public class WebServer
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly SharedControlsManager _sharedControlsManager;

		private string _rootPath;

		public WebServer(SharedControlsManager sharedControlsManager)
		{
			_sharedControlsManager = sharedControlsManager;

			if (!HttpListener.IsSupported)
				throw new NotSupportedException(
					"Needs Windows XP SP2, Server 2003 or later.");

			const int port = 80;
			_listener.Prefixes.Add("http://*:" + port + "/");

			//netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user

			try
			{
				_listener.Start();
			}
			catch
			{
				throw new Exception("Run this: netsh http add urlacl url=http://*:80/ user={USER}");
			}
			//Process.Start("http://127.0.0.1:" + port);

			_rootPath = "web";
			for (var i = 0; i < 8; i++)
			{
				if (Directory.Exists(_rootPath))
					break;
				_rootPath = "../" + _rootPath;
			}
		}

		public void Run()
		{
			ThreadPool.SetMaxThreads(100, 100);

			ThreadPool.QueueUserWorkItem((o) =>
			{
				Console.WriteLine("Webserver running...");
				try
				{
					while (_listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem((c) =>
						{
							var ctx = c as HttpListenerContext;
							
							object res = ProcessRequest(ctx.Request);

							byte[] buf = null;

							if (res is string)
								buf = Encoding.UTF8.GetBytes((string)res);
							else
								buf = (byte[])res;

							try
							{
								if (ctx.Request.Url.LocalPath.ToLower().EndsWith(".css"))
									ctx.Response.ContentType = "text/css";

								ctx.Response.ContentLength64 = buf.Length;
								ctx.Response.OutputStream.Write(buf, 0, buf.Length);

								// always close the stream
								ctx.Response.OutputStream.Close();
							}
							catch
							{
							}
						}, _listener.GetContext());
					}
				}
				catch { } // suppress any exceptions
			});
		}

		private object ProcessRequest(HttpListenerRequest request)
		{

			if (request.Url.LocalPath.StartsWith("/join"))
			{
				try
				{
					var p = _sharedControlsManager.Join();

					return JsonConvert.SerializeObject(new
					{
						playerNumber = p.Player.PlayerNumber,
						id = p.Id,
						color = p.Color
					});
				}
				catch (Exception ex)
				{
					return "error join";
				}
			}

			if (request.Url.LocalPath.StartsWith("/update"))
			{
				try
				{
					var json = new StreamReader(request.InputStream).ReadToEnd();
					Console.WriteLine(json);
					var packet = JsonConvert.DeserializeObject<ControlPacket>(json);
					_sharedControlsManager.Update(packet);
					return DateTime.UtcNow.Ticks.ToString();
				}
				catch (Exception ex)
				{
					return "error update";
				}
			}

			if (request.Url.LocalPath.StartsWith("/status"))
			{
				try
				{
					var json = new StreamReader(request.InputStream).ReadToEnd();
					Console.WriteLine(json);
					var packet = JsonConvert.DeserializeObject<BasePacket>(json);
					return JsonConvert.SerializeObject(_sharedControlsManager.StatusPoll(packet));

				}
				catch (Exception ex)
				{
					return "error status";
				}
			}

			var fileName = request.Url.LocalPath.Substring(1);
			if (fileName == "")
				fileName = "index.html";
			fileName = Path.Combine(_rootPath, fileName);
			if (File.Exists(fileName))
			{
				if (fileName.EndsWith(".html") || fileName.EndsWith(".js") || fileName.EndsWith(".css"))
					return File.ReadAllText(fileName);
				return File.ReadAllBytes(fileName);
			}

			return "error You requested " + request.Url;
		}

		public void Stop()
		{
			_listener.Stop();
			_listener.Close();
		}
	}
}
