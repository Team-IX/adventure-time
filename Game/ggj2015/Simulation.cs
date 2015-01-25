using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using ggj2015.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input.Devices;

namespace ggj2015
{
	class Simulation
	{
		private const int GamePadCount = 4;

		public Player[] Players;

		public readonly List<Bomb> Bombs = new List<Bomb>();
		public readonly List<Explosion> Explosions = new List<Explosion>();
		public readonly List<PowerUp> PowerUps = new List<PowerUp>();
		private readonly PlayerPerson[] _gamePadPlayerPerson = new PlayerPerson[GamePadCount];
		private TimeSpan? _lastEveryBodySwap;

		public void InitialPopulate()
		{
			Bombs.Clear();
			Explosions.Clear();
			PowerUps.Clear();

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
				"#333333"
			};

			Players = new Player[Player.Count];
			for (var i = 0; i < Player.Count; i++)
			{

				//var limit = new VelocityLimitController(PlayerMaximumVelocity, 0);
				//Globals.World.AddController(limit);
				//limit.AddBody(playerBody);

				Players[i] = new Player(i, colors[i], playerCellStarts[i]);
			}
		}

		public void CreatePlayerPersonForGamepads()
		{
			for (var i = 0; i < GamePadCount; i++)
			{
				var pp = Globals.Controls.Join();
				_gamePadPlayerPerson[i] = pp;
			}
		}

		public void Update()
		{
			for (var i = 0; i < Player.Count; i++)
			{
				if (Players[i].IsAlive)
				{
					Players[i].ApplyMovementForce();
					Players[i].BombUpdate();
				}
			}
			foreach (var bomb in Bombs.ToArray())
			{
				bomb.ExplodeMaybe();
			}

			foreach (var e in Explosions.ToArray())
			{
				e.Update();
				if (e.IsFinished())
				{
					Explosions.Remove(e);
				}
			}

			Bombs.RemoveAll(x => x.HasExploded);
		}

		public void UpdateControls()
		{
			if (_gamePadPlayerPerson[0] == null)
				return;

			for (var i = 0; i < GamePadCount; i++)
			{
				var gps = Globals.Input.GamePads[i].GetState();
				if (!Globals.Input.GamePads[i].IsAttached)
					continue;

				List<Control> controls = new List<Control>();
				if (gps.DPad.Down == ButtonState.Pressed)
					controls.Add(Control.Down);
				if (gps.DPad.Left == ButtonState.Pressed)
					controls.Add(Control.Left);
				if (gps.DPad.Up == ButtonState.Pressed)
					controls.Add(Control.Up);
				if (gps.DPad.Right == ButtonState.Pressed)
					controls.Add(Control.Right);
				if (gps.IsButtonDown(Buttons.A))
					controls.Add(Control.Bomb);

				Globals.Controls.Update(new ControlPacket(_gamePadPlayerPerson[i].Id, controls.ToArray()));
			}
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

		public void Render()
		{
			//Walls
			for (var y = -1; y < GameWorld.Height + 1; y++)
			{
				for (var x = -1; x < GameWorld.Width + 1; x++)
				{
					if (x == -1 || y == -1 || x == GameWorld.Width || y == GameWorld.Height)
						Globals.SpriteBatch.DrawTile(Resources.Objects.Wall, ConvertUnits.ToDisplayUnits(new Vector2(x, y) * GameWorld.CellSize));
				}
			}


			//Other things
			for (var y = 0; y < GameWorld.Height; y++)
			{
				for (var x = 0; x < GameWorld.Width; x++)
				{
					var obj = Globals.GameWorld.ObjectsInCells[x, y];

					if (obj is UnbreakableWall)
					{
						Globals.SpriteBatch.DrawTile(Resources.Objects.Unbreakable, ConvertUnits.ToDisplayUnits(obj.Body.Position));
					}
					else if (obj is BreakableWall)
					{
						Globals.SpriteBatch.DrawTile(Resources.Objects.Breakable, ConvertUnits.ToDisplayUnits(obj.Body.Position));
					}
				}
			}

			foreach (var bomb in Bombs)
			{
				bomb.Render();
			}
			foreach (var powerUp in PowerUps)
			{
				powerUp.Render();
			}
			foreach (var explosion in Explosions)
			{
				explosion.Render();
			}
			foreach (var player in Players)
			{
				player.Render();
			}

			if (IsSwapActive)
			{
				for (var i = 0; i < 6; i++)
					Globals.SpriteBatch.DrawStringCentered(Resources.Font200, "WHAT DO WE DO NOW !>#$??", new Vector2(Globals.RenderWidth / 2, 50 + i * 100), new Color(Globals.Random.Next(50, 255), Globals.Random.Next(50, 255), Globals.Random.Next(50, 255)), 0.3f);
			}

			for (var i = 0; i < Players.Length; i++)
			{
				var p = Players[i];
				var count = Globals.Controls.CountPeopleForPlayer(p);
				Globals.SpriteBatch.DrawString(Resources.Font200, p.ColorStr + ": " + count, new Vector2(10, 50 + i * 30), p.Color, 0, Vector2.Zero, 0.08f, SpriteEffects.None, 0);
			}
		}

		public bool IsSwapActive
		{
			get { return _lastEveryBodySwap.HasValue && _lastEveryBodySwap.Value > Globals.GameTime.TotalGameTime - TimeSpan.FromSeconds(1); }
		}

		public void Reset()
		{
			Globals.World.Clear();
			Globals.GameWorld.InitialPopulate();
			Globals.Simulation.InitialPopulate();
			Globals.Controls.Reset();
			//Globals.Simulation.CreatePlayerPersonForGamepads();
			
			_lastEveryBodySwap = null;
		}

		public void EverybodySwapTriggered()
		{
			_lastEveryBodySwap = Globals.GameTime.TotalGameTime;
		}
	}
}
