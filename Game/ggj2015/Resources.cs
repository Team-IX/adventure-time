using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ggj2015
{
	static class Resources
	{
		public static Texture2D Test;

		public static class PowerUps
		{
			public static Texture2D BiggerExplosions, EverybodySwap, MoreBombs, MoreSpeed;
		}

		public static class Objects
		{
			public static Texture2D Breakable, Unbreakable, Bomb, BackgroundTile, Wall;
		}

		public static class Player
		{
			public static SpriteSet Left, Right, Up, Down;
			public static Texture2D BackDead, ForeDead;
		}

		public static void Load(ContentManager content)
		{
			Test = content.Load<Texture2D>("test");

			PowerUps.BiggerExplosions = content.Load<Texture2D>("powerups/BiggerExplosions");
			PowerUps.EverybodySwap = content.Load<Texture2D>("powerups/EverybodySwap");
			PowerUps.MoreBombs = content.Load<Texture2D>("powerups/MoreBombs");
			PowerUps.MoreSpeed = content.Load<Texture2D>("powerups/MoreSpeed");

			Objects.Breakable = content.Load<Texture2D>("objects/grass");
			Objects.Unbreakable = content.Load<Texture2D>("objects/stone");

			Objects.Bomb = content.Load<Texture2D>("objects/bomb");

			Objects.BackgroundTile = content.Load<Texture2D>("objects/bricks_tile");
			Objects.Wall = content.Load<Texture2D>("objects/wall");


			Player.Left = new SpriteSet(content, "player/left/");
			Player.Right = new SpriteSet(content, "player/right/");
			Player.Up = new SpriteSet(content, "player/up/");
			Player.Down = new SpriteSet(content, "player/down/");

			Player.BackDead = content.Load<Texture2D>("player/bgdead");
			Player.ForeDead = content.Load<Texture2D>("player/dead");
		}
	}

	public class SpriteSet
	{
		public Texture2D[] Fore;
		public Texture2D[] Back;

		public SpriteSet(ContentManager content, string dir)
		{
			Fore = new Texture2D[]
			{
				content.Load<Texture2D>(dir + "1"),
				content.Load<Texture2D>(dir + "2"),
				content.Load<Texture2D>(dir + "3"),
				content.Load<Texture2D>(dir + "4")
			};


			Back = new Texture2D[]
			{
				content.Load<Texture2D>(dir + "bg1"),
				content.Load<Texture2D>(dir + "bg2"),
				content.Load<Texture2D>(dir + "bg3"),
				content.Load<Texture2D>(dir + "bg4")
			};
		}
	}
}
