using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace ggj2015.GameObjects
{
	//http://bomberman.wikia.com/wiki/Power-Ups
	public enum PowerUpType
	{
		MoreBombs,
		BiggerExplosions,
		MoreSpeed,
		EverybodySwap,


		Count
	}

	class PowerUp : GameObject
	{
		private readonly int _gridX;
		private readonly int _gridY;
		public PowerUpType Type;
		public Body Body { get; set; }
		public TimeSpan TimeCreated { get; set; }

		private bool _alreadyCollected;

		public static readonly TimeSpan PowerUpInvulnerabilityStartTime = TimeSpan.FromSeconds(0.1f);

		public PowerUp(int gridX, int gridY)
		{
			TimeCreated = Globals.GameTime.TotalGameTime;

			_gridX = gridX;
			_gridY = gridY;
			Type = (PowerUpType)Globals.Random.Next(0, (int)PowerUpType.Count);

			Body = BodyFactory.CreateRectangle(Globals.World, GameWorld.CellSize * 0.1f, GameWorld.CellSize * 0.1f, 0, new Vector2(gridX, gridY) * GameWorld.CellSize, 0, BodyType.Static, this);

			Body.OnCollision += Body_OnCollision;
		}


		bool Body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (_alreadyCollected)
				return false;

			var otherBody = (fixtureA.Body == Body) ? fixtureB.Body : fixtureA.Body;

			if (otherBody.UserData is Player)
			{
				var p = (Player)otherBody.UserData;
				switch (Type)
				{
					case PowerUpType.BiggerExplosions:
						p.BombExplosionSize++;
						break;
					case PowerUpType.MoreBombs:
						p.MaxBombs++;
						break;
					case PowerUpType.MoreSpeed:
						p.PlayerMovementForce += Player.PlayerMovementForceIncrement;
						break;
					case PowerUpType.EverybodySwap:
						Globals.Controls.EverybodySwap();
						break;
					default:
						throw new Exception();
				}

				Destroy();
			}

			return false;
		}

		public void Destroy()
		{
			if (_alreadyCollected)
				return;

			_alreadyCollected = true;

			Globals.World.RemoveBody(Body);

			Globals.GameWorld.ObjectsInCells[_gridX, _gridY] = null;
		}
	}
}
