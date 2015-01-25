using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

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

		public static Texture2D[] ExplosionAnim;

		public static SpriteFont Font200;

		public static Texture2D Px;

		public static Song Music;
		public static SoundEffect SoundExplosion, Death;

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

			const int count = 22;
			ExplosionAnim = new Texture2D[count];
			for (var i = 0; i < count; i++)
			{
				ExplosionAnim[i] = content.Load<Texture2D>("explosion/anim/explosion" + (i + 1));
			}

			Px = content.Load<Texture2D>("1px");

			Font200 = content.Load<SpriteFont>("Cocogoose_200");

			Music = content.Load<Song>("audio/music");
			MediaPlayer.Volume = 0.5f;
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Play(Music);
			MediaPlayer.IsRepeating = true;

			SoundExplosion = content.Load<SoundEffect>("audio/explosion");
			Death = content.Load<SoundEffect>("audio/death");
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
