using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace ggj2015.GameObjects
{
	class Bomb : GameObject
	{
		public readonly Player Player;


		private readonly List<Body> _allowedToPassThrough;
		private readonly List<Body> _hitThisFrame = new List<Body>(); 


		public int X { get; set; }
		public int Y { get; set; }
		public readonly TimeSpan PlacedTime;
		public TimeSpan LifeTime { get; set; }
		public int ExplosionSize { get; set; }

		public Body Body { get; set; }

		public bool HasExploded { get; private set; }

		public Bomb(Player player, int x, int y, TimeSpan placedTime)
		{
			X = x;
			Y = y;

			PlacedTime = placedTime;
			LifeTime = player.BombLifeTime;
			ExplosionSize = player.BombExplosionSize;
			Player = player;

			Vector2 center = new Vector2(x, y) * GameWorld.CellSize;

			//Figure out what we are colliding with at the start
			var aabb = new AABB(center, GameWorld.CellSize, GameWorld.CellSize);
			var hits = Globals.World.QueryAABB(ref aabb);

			_allowedToPassThrough = hits.Select(f => f.Body).Where(b => b.UserData is Player).ToList();

			Body = BodyFactory.CreateCircle(Globals.World, GameWorld.CellSize / 2, 0, center, BodyType.Static, this);
			Body.UserData = this;

			Body.OnCollision += BodyOnOnCollision;
		}



		private bool BodyOnOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			var otherBody = (fixtureA.Body == Body) ? fixtureB.Body : fixtureA.Body;

			if (_allowedToPassThrough.Contains(otherBody))
			{
				_hitThisFrame.Add(otherBody);
				return false;
			}

			return true;
		}

		public void PostPhysicsUpdate()
		{
			_allowedToPassThrough.RemoveAll(x => !_hitThisFrame.Contains(x));
			_hitThisFrame.Clear();
		}

		public void ExplodeMaybe()
		{
			if (Globals.GameTime.TotalGameTime < PlacedTime + LifeTime)
				return;

			ForceExplode();
		}

		public void ForceExplode()
		{
			if (HasExploded)
				return;

			//Scan to the left
			int minX = X;
			bool hitSomething = false;
			for (minX = X; minX >= X - ExplosionSize && minX >= 0; minX--)
			{
				var o = Globals.GameWorld.ObjectsInCells[minX, Y];
				if (o == null)
					continue;

				hitSomething = true;
				break;
			}
			if (!hitSomething || Globals.GameWorld.ObjectsInCells[minX, Y] is UnbreakableWall)
				minX++;

			//Scan to the right
			int maxX = X;
			hitSomething = false;
			for (maxX = X; maxX <= X + ExplosionSize && maxX < GameWorld.Width; maxX++)
			{
				var o = Globals.GameWorld.ObjectsInCells[maxX, Y];
				if (o == null)
					continue;

				hitSomething = true;
				break;
			}
			if (!hitSomething || Globals.GameWorld.ObjectsInCells[maxX, Y] is UnbreakableWall)
				maxX--;

			//Scan to the up
			int minY = Y;
			hitSomething = false;
			for (minY = Y; minY >= Y - ExplosionSize && minY >= 0; minY--)
			{
				var o = Globals.GameWorld.ObjectsInCells[X, minY];
				if (o == null)
					continue;

				hitSomething = true;
				break;
			}
			if (!hitSomething || Globals.GameWorld.ObjectsInCells[X, minY] is UnbreakableWall)
				minY++;

			//Scan to the down
			int maxY = Y;
			hitSomething = false;
			for (maxY = Y; maxY <= Y + ExplosionSize && maxY < GameWorld.Height; maxY++)
			{
				var o = Globals.GameWorld.ObjectsInCells[X, maxY];
				if (o == null)
					continue;

				hitSomething = true;
				break;
			}
			if (!hitSomething || Globals.GameWorld.ObjectsInCells[X, maxY] is UnbreakableWall)
				maxY--;

			//Destroy what we hit if breakable
			Globals.GameWorld.DestroyMaybe(minX, Y);
			Globals.GameWorld.DestroyMaybe(maxX, Y);
			Globals.GameWorld.DestroyMaybe(X, maxY);
			Globals.GameWorld.DestroyMaybe(X, minY);


			//Remove ourself and make an explosion which handles the killing
			HasExploded = true;
			Globals.World.RemoveBody(Body);
			Player.PlacedBombs--;
			Globals.Simulation.Explosions.Add(new Explosion(X, Y, minX, maxX, minY, maxY));
		}
	}
}
