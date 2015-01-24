using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
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

			//Remove ourself and make an explosion which handles the killing
			HasExploded = true;
			Globals.World.RemoveBody(Body);
			Player.PlacedBombs--;
			Globals.Simulation.Explosions.Add(new Explosion(X, Y, ExplosionSize));
		}

		public void Render()
		{
			Globals.SpriteBatch.DrawTile(Resources.Objects.Bomb, ConvertUnits.ToDisplayUnits(Body.Position));
			//Globals.SpriteBatch.Draw(Resources.Objects.Bomb, ConvertUnits.ToDisplayUnits(Body.Position), origin: Resources.Test.CenteredOrigin());
		}
	}
}
