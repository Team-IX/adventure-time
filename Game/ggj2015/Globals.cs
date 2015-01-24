using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace ggj2015
{
	static class Globals
	{
		public const int RenderWidth = 1280;
		public const int RenderHeight = 720;

		public static World World;
		public static InputManager Input;

		public static SharedControlsManager Controls { get; set; }
		public static WebServer WebServer { get; set; }
		public static GameWorld GameWorld { get; set; }
		public static Simulation Simulation { get; set; }
		public static GameTime GameTime { get; set; }
		public static SpriteBatch SpriteBatch { get; set; }

		public static readonly Random Random = new Random();

		public static GameState State = GameState.PreGame;

		public const float TilePx = 52;
	}
}
