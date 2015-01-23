using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace ggj2015
{
	public class Player
	{
		public const int Count = 1;

		const float PlayerMovementForce = 0.04f;


		public string Color { get; set; }
		public Body Body { get; set; }

		public readonly int PlayerNumber;

		public int PersonCount;

		private readonly Dictionary<int, ControlPacket> _votes = new Dictionary<int, ControlPacket>(); 

		public Player(int playerNumber, string color, Body body)
		{
			Color = color;
			Body = body;
			PlayerNumber = playerNumber;
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
				else if (movement == Control.Up)
					Body.ApplyForce(new Vector2(0, -PlayerMovementForce));
				else if (movement == Control.Down)
					Body.ApplyForce(new Vector2(0, PlayerMovementForce));
			}
			else if (movement == Control.Left || movement == Control.Right)
			{
				var y = (int)Math.Round(pos.Y / GameWorld.CellSize);
				var yMod = (pos.Y % (GameWorld.CellSize * 2)) / (GameWorld.CellSize * 2);

				if (yMod > movementFudge && yMod < 0.5f) //Need to move up a bit
					Body.ApplyForce(new Vector2(0, -PlayerMovementForce));
				else if (yMod > 0.5f && yMod < 1 - movementFudge) // Need to move down a bit
					Body.ApplyForce(new Vector2(0, PlayerMovementForce));
				else if (movement == Control.Left)
					Body.ApplyForce(new Vector2(-PlayerMovementForce, 0));
				else if (movement == Control.Right)
					Body.ApplyForce(new Vector2(PlayerMovementForce, 0));
			}
		}
	}
}
