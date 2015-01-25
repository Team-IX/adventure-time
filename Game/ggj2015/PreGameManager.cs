using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ggj2015
{
	class PreGameManager
	{
		public void Render()
		{
			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Adventure Time", new Vector2(Globals.RenderWidth / 2f, 100), Color.White, 0.3f);

			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Players: " + Globals.Controls.CurrentCount, new Vector2(Globals.RenderWidth / 2f, 200), Color.White, 0.2f);


			var net = NetworkInterface.GetAllNetworkInterfaces();

			var ips = net
				.Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(x => x.GetIPProperties())
				.SelectMany(x => x.UnicastAddresses)
				.Select(x => x.Address.ToString())
				.Where(x => !x.StartsWith("169.") && !x.Contains(":"))
				.ToArray();

			for (int i = 0; i < ips.Length; i++)
			{
				var ip = ips[i];
				Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "http://" + ip, new Vector2(Globals.RenderWidth / 2f, 600 - i * 100), Color.White, 0.3f);
			}

			if (Globals.Controls.CurrentCount > 0)
			{
				Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Enter to Start!", new Vector2(Globals.RenderWidth / 2f, 350), Color.White, 0.3f);
			}
		}

		public void Update(KeyboardState[] states)
		{
			if (states.Any(s => s.IsKeyDown(Keys.Enter)))
			{
				Globals.State = GameState.PlayingGame;
			}
		}
	}
}
