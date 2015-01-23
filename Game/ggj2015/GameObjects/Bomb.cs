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

		public Body Body { get; set; }

		public Bomb(Player player, int x, int y)
		{
			X = x;
			Y = y;
			Player = player;
			Vector2 center = new Vector2(x, y) * GameWorld.CellSize;

			//Figure out what we are colliding with at the start
			var aabb = new AABB(center, GameWorld.CellSize, GameWorld.CellSize);
			var hits = Globals.World.QueryAABB(ref aabb);

			_allowedToPassThrough = hits.Select(f => f.Body).Where(b => b.UserData is Player).ToList();

			Body = BodyFactory.CreateCircle(Globals.World, GameWorld.CellSize / 2, 0, center, BodyType.Static, this);

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
	}
}
