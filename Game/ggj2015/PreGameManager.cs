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
			Globals.SpriteBatch.Draw(Resources.Px, new Rectangle(0, 0, Globals.RenderWidth, Globals.RenderHeight), new Color(0, 0, 0, 80));
			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Crowd bomb", new Vector2(Globals.RenderWidth / 2f, 100), Color.White, 0.3f);

			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Players: " + Globals.Controls.CurrentCount, new Vector2(Globals.RenderWidth / 2f, 200), Color.White, 0.2f);

#if true
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
				Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "http://" + ip, new Vector2(Globals.RenderWidth / 2f, 300 + i * 100), Color.White, 0.3f);
			}
#else
			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Connect to our Wifi: GETINTHEGAME", new Vector2(Globals.RenderWidth / 2f, 300), Color.White, 0.22f);
			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "http://ga.me", new Vector2(Globals.RenderWidth / 2f, 400), Color.White, 0.3f);

#endif
			if (Globals.Controls.CurrentCount > 0)
			{
				Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Enter to Start!", new Vector2(Globals.RenderWidth / 2f, 600), Color.White, 0.2f);
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
