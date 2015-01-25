using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace ggj2015
{
	class PostGameManager
	{
		public Player Winner { get; set; }
		public TimeSpan TimeShown { get; set; }

		public void Init()
		{
			Winner = Globals.Simulation.Players.Single(x => x.IsAlive);

			TimeShown = Globals.GameTime.TotalGameTime;
		}


		public void Render()
		{
			Globals.SpriteBatch.Draw(Resources.Px, new Rectangle(0, 0, Globals.RenderWidth, Globals.RenderHeight), new Color(0, 0, 0, 80));

			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Winner", new Vector2(Globals.RenderWidth / 2f, 100), Color.White, 0.3f);

			Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "Team: " + Winner.ColorStr, new Vector2(Globals.RenderWidth / 2f, 200), Winner.Color, 0.3f);
		}

		public void Update()
		{
			if (TimeShown < Globals.GameTime.TotalGameTime - TimeSpan.FromSeconds(5))
			{
				Globals.Simulation.Reset();
				Globals.State = GameState.PreGame;
			}
		}
	}
}
