using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ggj2015
{
	public class Player
	{
		public const int Count = 4;

		public float PlayerMovementForce = 0.045f;
		public const float PlayerMovementForceIncrement = 0.01f;

		public Color Color;
		public string ColorStr { get; set; }
		public Body Body { get; set; }

		public readonly int PlayerNumber;

		public int PersonCount;


		public int MaxBombs = 1;
		public int PlacedBombs = 0;

		public TimeSpan BombLifeTime { get; set; }
		public bool IsAlive = true;

		public int BombExplosionSize = 1;


		private readonly Dictionary<int, ControlPacket> _votes = new Dictionary<int, ControlPacket>(); 

		public Player(int playerNumber, string colorStr, Vector2 startPos)
		{
			ColorStr = colorStr;
			Color = new Color(
				Convert.ToInt32(colorStr.Substring(1, 2), 16),
				Convert.ToInt32(colorStr.Substring(3, 2), 16),
				Convert.ToInt32(colorStr.Substring(5, 2), 16),
				255);

			PlayerNumber = playerNumber;

			BombLifeTime = TimeSpan.FromSeconds(3);

			Body = BodyFactory.CreateRoundedRectangle(Globals.World,
				GameWorld.CellSize * GameWorld.PlayerScale, GameWorld.CellSize * GameWorld.PlayerScale,
				GameWorld.PlayerRadius, GameWorld.PlayerRadius,
				4, 0.01f,
				new Vector2(startPos.X * GameWorld.CellSize, startPos.Y * GameWorld.CellSize), bodyType: BodyType.Dynamic);
			Body.FixedRotation = true;
			Body.LinearDamping = 20;
			Body.Friction = 0;
			Body.SleepingAllowed = false;
			Body.UserData = this;
		}

		public void ConsumePacket(ControlPacket packet)
		{
			lock (this)
			{
				_votes[packet.Id] = packet;
			}
		}

		private Control ResolveMovementVote()
		{
			lock (this)
			{
				var votes = _votes.SelectMany(x => x.Value.Controls).Where(x => x != Control.Bomb).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

				if (votes.Count == 0)
					return Control.NoMovement;

				var mostVotes = votes.Max(x => x.Value);

				return votes.First(x => x.Value == mostVotes).Key;
			}
		}

		private bool ResolveBombVote()
		{
			lock (this)
			{
				return _votes.Any(x => x.Value.Controls.Contains(Control.Bomb));
			}
		}

		public void ApplyMovementForce()
		{
			const float movementFudge = 0.02f;

			var movement = ResolveMovementVote();

			var pos = Body.Position;
			if (movement == Control.Up || movement == Control.Down)
			{
				var x = (int)Math.Round(pos.X / GameWorld.CellSize);
				var xMod = (pos.X % (GameWorld.CellSize * 2)) / (GameWorld.CellSize * 2);

				if (xMod > movementFudge && xMod < 0.5f) //Need to move left a bit
					Body.ApplyForce(new Vector2(-PlayerMovementForce, 0));
				else if (xMod > 0.5f && xMod < 1 - movementFudge) // Need to move right a bit
					Body.ApplyForce(new Vector2(PlayerMovementForce, 0));

				if (Math.Abs(xMod) < 0.1f || Math.Abs(xMod) > 0.9f)
				{
					Body.LinearVelocity = Body.LinearVelocity * new Vector2(0, 1);
					if (movement == Control.Up)
						Body.ApplyForce(new Vector2(0, -PlayerMovementForce));
					else if (movement == Control.Down)
						Body.ApplyForce(new Vector2(0, PlayerMovementForce));
				}
			}
			else if (movement == Control.Left || movement == Control.Right)
			{
				var y = (int)Math.Round(pos.Y / GameWorld.CellSize);
				var yMod = (pos.Y % (GameWorld.CellSize * 2)) / (GameWorld.CellSize * 2);

				if (yMod > movementFudge && yMod < 0.5f) //Need to move up a bit
					Body.ApplyForce(new Vector2(0, -PlayerMovementForce));
				else if (yMod > 0.5f && yMod < 1 - movementFudge) // Need to move down a bit
					Body.ApplyForce(new Vector2(0, PlayerMovementForce));

				if (Math.Abs(yMod) < 0.1f || Math.Abs(yMod) > 0.9f)
				{
					Body.LinearVelocity = Body.LinearVelocity * new Vector2(1, 0);
					if (movement == Control.Left)
						Body.ApplyForce(new Vector2(-PlayerMovementForce, 0));
					else if (movement == Control.Right)
						Body.ApplyForce(new Vector2(PlayerMovementForce, 0));
				}
			}
		}

		public void BombUpdate()
		{
			if (!ResolveBombVote())
				return;

			if (PlacedBombs < MaxBombs)
			{
				var pos = Body.Position;
				if (Globals.Simulation.TryCreateBomb(this, (int)Math.Round(pos.X / GameWorld.CellSize), (int)Math.Round(pos.Y / GameWorld.CellSize)))
				{
					PlacedBombs++;
				}
			}
		}

		public void Die()
		{
			IsAlive = false;
			//todo
			Body.BodyType = BodyType.Static;
		}

		public void ResetVotes()
		{
			_votes.Clear();
		}


		private float _spriteIndex;

		public void Render()
		{
			Vector2 offset = new Vector2(0, 34);

			//Direction

			var vel = Body.LinearVelocity;
			const float req = 0.01f;

			Texture2D back, fore;

			if (IsAlive)
			{
				var sprites = Resources.Player.Down;

				if (vel.X > req)
					sprites = Resources.Player.Right;
				else if (vel.X < -req)
					sprites = Resources.Player.Left;
				else if (vel.Y > req)
					sprites = Resources.Player.Down;
				else if (vel.Y < -req)
					sprites = Resources.Player.Up;

				if (vel.Length() < req)
					_spriteIndex = 0;
				else
					_spriteIndex = (_spriteIndex + vel.Length() * (float)Globals.GameTime.ElapsedGameTime.TotalSeconds * 10) % sprites.Back.Length;

				back = sprites.Back[(int)_spriteIndex];
				fore = sprites.Fore[(int)_spriteIndex];
			}
			else
			{
				back = Resources.Player.BackDead;
				fore = Resources.Player.ForeDead;
			}

			Globals.SpriteBatch.DrawTile(back, ConvertUnits.ToDisplayUnits(Body.Position) - offset, Color);
			Globals.SpriteBatch.DrawTile(fore, ConvertUnits.ToDisplayUnits(Body.Position) - offset);
		}
	}
}
