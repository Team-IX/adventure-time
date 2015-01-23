﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ggj2015
{
	public class WebServer
	{
		private readonly HttpListener _listener = new HttpListener();
		private readonly SharedControlsManager _sharedControlsManager;

		public WebServer(SharedControlsManager sharedControlsManager)
		{
			_sharedControlsManager = sharedControlsManager;

			if (!HttpListener.IsSupported)
				throw new NotSupportedException(
					"Needs Windows XP SP2, Server 2003 or later.");

			_listener.Prefixes.Add("http://*:" + 12543 + "/");

			//netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user

			_listener.Start();

			//TODOD Process.Start("http://127.0.0.1:12543");
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

							ctx.Response.ContentLength64 = buf.Length;
							ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							
							// always close the stream
							ctx.Response.OutputStream.Close();
						}, _listener.GetContext());
					}
				}
				catch { } // suppress any exceptions
			});
		}

		private object ProcessRequest(HttpListenerRequest request)
		{
			if (request.Url.LocalPath.StartsWith("/static/"))
			{
				var fileName = "static/" + request.Url.LocalPath.Substring("/static/".Length);
				if (File.Exists(fileName))
				{
					if (fileName.EndsWith(".html") || fileName.EndsWith(".js") || fileName.EndsWith(".css"))
						return File.ReadAllText(fileName);
					return File.ReadAllBytes(fileName);
				}
			}

			if (request.Url.LocalPath.StartsWith("/join"))
			{
				var p = _sharedControlsManager.Join();

				return JsonConvert.SerializeObject(new
				{
					playerNumber = p.Player.PlayerNumber,
					id = p.Id,
					color = p.Color
				});
			}

			if (request.Url.LocalPath.StartsWith("/update"))
			{
				try
				{
					var json = new StreamReader(request.InputStream).ReadToEnd();
					Console.WriteLine(json);
					var packet = JsonConvert.DeserializeObject<ControlPacket>(json);
					_sharedControlsManager.Update(packet);
				}
				catch (Exception ex)
				{
					//Console.WriteLine("Failed to get update: " + ex);
				}
			}

			return "You requested " + request.Url;
		}

		public void Stop()
		{
			_listener.Stop();
			_listener.Close();
		}
	}
}