using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using ggj2015.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input.Devices;

namespace ggj2015
{
	class Simulation
	{
		public Player[] Players;

		public readonly List<Bomb> Bombs = new List<Bomb>();
		public readonly List<Explosion> Explosions = new List<Explosion>();

		public void InitialPopulate()
		{
			var playerCellStarts = new[]
			{
				new Vector2(0, 0),
				new Vector2(GameWorld.Width - 1, GameWorld.Height - 1),

				new Vector2(GameWorld.Width - 1, 0),
				new Vector2(0, GameWorld.Height - 1),
			};
			var colors = new[]
			{
				"#ff0000",
				"#00ff00",
				"#0000ff",
				"#ffffff"
			};

			Players = new Player[Player.Count];
			for (var i = 0; i < Player.Count; i++)
			{
				var pos = playerCellStarts[i];
				var playerBody = BodyFactory.CreateRoundedRectangle(Globals.World,
					GameWorld.CellSize * GameWorld.PlayerScale, GameWorld.CellSize * GameWorld.PlayerScale,
					GameWorld.PlayerRadius, GameWorld.PlayerRadius,
					4, 0.01f,
					new Vector2(pos.X * GameWorld.CellSize, pos.Y * GameWorld.CellSize), bodyType: BodyType.Dynamic);
				playerBody.FixedRotation = true;
				playerBody.LinearDamping = 20;
				playerBody.Friction = 0;
				playerBody.SleepingAllowed = false;

				//var limit = new VelocityLimitController(PlayerMaximumVelocity, 0);
				//Globals.World.AddController(limit);
				//limit.AddBody(playerBody);

				Players[i] = new Player(i, colors[i], playerBody);
			}
		}

		public void Update()
		{
			foreach (var bomb in Bombs.ToArray())
			{
				bomb.ExplodeMaybe();
			}

			foreach (var e in Explosions.ToArray())
			{
				if (e.IsFinished())
				{
					Explosions.Remove(e);
				}
			}

			Bombs.RemoveAll(x => x.HasExploded);
		}


		public void UpdateControls(GameTime gameTime)
		{
			var gps1 = Globals.Input.GamePads[0].GetState();

			List<Control> controls = new List<Control>();
			if (gps1.DPad.Down == ButtonState.Pressed)
				controls.Add(Control.Down);
			if (gps1.DPad.Left == ButtonState.Pressed)
				controls.Add(Control.Left);
			if (gps1.DPad.Up == ButtonState.Pressed)
				controls.Add(Control.Up);
			if (gps1.DPad.Right == ButtonState.Pressed)
				controls.Add(Control.Right);
			if (gps1.IsButtonDown(Buttons.A))
				controls.Add(Control.Bomb);

			Players[0].ConsumePacket(new ControlPacket(0, controls.ToArray()));


			Players[0].ApplyMovementForce();
			Players[0].BombUpdate(gameTime);
		}

		public bool TryCreateBomb(Player player, int x, int y)
		{
			if (Bombs.Any(b => b.X == x && b.Y == y))
				return false;
			Bombs.Add(new Bomb(player, x, y, Globals.GameTime.TotalGameTime));
			return true;
		}

		public void PostPhysicsUpdate()
		{
			foreach (var bomb in Bombs)
				bomb.PostPhysicsUpdate();
		}
	}
}
