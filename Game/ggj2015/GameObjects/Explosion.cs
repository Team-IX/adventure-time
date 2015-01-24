using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ggj2015.GameObjects
{
	internal class ExplosionBody
	{
		public int X { get; set; }
		public int Y { get; set; }
		public Body Body;
		public TimeSpan Created;

		public static readonly TimeSpan LifeTime = TimeSpan.FromSeconds(0.4f);
		public ExplosionBody(Body body, TimeSpan created, int x, int y)
		{
			X = x;
			Y = y;
			Body = body;
			Created = created;
		}
	}

	class Explosion : GameObject
	{
		private readonly int _centerX;
		private readonly int _centerY;
		private readonly int _explosionSize;
		private readonly int _minX;
		private readonly int _maxX;
		private readonly int _minY;
		private readonly int _maxY;

		private const float Padding = GameWorld.CellSize * 0.4f;

		public readonly List<ExplosionBody> Bodies = new List<ExplosionBody>();
		private TimeSpan _lastExplosionCreatedTime;
		private int _explosionsCreated;

		private bool _leftStillGoing = true;
		private bool _rightStillGoing = true;
		private bool _upStillGoing = true;
		private bool _downStillGoing = true;

		public Explosion(int centerX, int centerY, int explosionSize)
		{
			_centerX = centerX;
			_centerY = centerY;
			_explosionSize = explosionSize;

			_lastExplosionCreatedTime = Globals.GameTime.TotalGameTime;

			CreateExplosion(centerX, centerY);
		}

		private bool TryCreateExplosion(int x, int y)
		{
			if (x < 0 || y < 0 || x >= GameWorld.Width || y >= GameWorld.Height)
				return false;

			var hit = Globals.GameWorld.ObjectsInCells[x, y];
			if (hit is UnbreakableWall)
				return false;

			//Already an explosion here
			//if (Globals.Simulation.Explosions.Any(b => b.Bodies.Any(e => e.X == x && e.Y == y)))
			//	return false;

			CreateExplosion(x, y);

			var otherBomb = Globals.Simulation.Bombs.FirstOrDefault(b => b.X == x && b.Y == y);
			if (otherBomb != null)
			{
				otherBomb.ForceExplode();
				//return false;
			}

			if (hit is BreakableWall)
			{
				Globals.GameWorld.DestroyMaybe(x, y);

				return false;
			}

			return true;
		}

		private void CreateExplosion(int x, int y)
		{
			var body = BodyFactory.CreateRectangle(Globals.World, GameWorld.CellSize - Padding, GameWorld.CellSize - Padding, 0, new Vector2(x * GameWorld.CellSize, y * GameWorld.CellSize), 0, BodyType.Dynamic, this);
			body.SleepingAllowed = false;
			body.IsSensor = true;
			body.OnCollision += BodyOnOnCollision;

			Bodies.Add(new ExplosionBody(body, Globals.GameTime.TotalGameTime, x, y));
		}


		private bool BodyOnOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			var otherBody = Bodies.Any(x => x.Body == fixtureA.Body) ? fixtureB.Body : fixtureA.Body;

			var part = Bodies.First(x => x.Body == fixtureA.Body || x.Body == fixtureB.Body);

			if (otherBody.UserData is Player)
				((Player)otherBody.UserData).Die();
			if (otherBody.UserData is Bomb)
				((Bomb)otherBody.UserData).ForceExplode();
			if (otherBody.UserData is PowerUp)
			{
				var p = (PowerUp)otherBody.UserData;
				if (part.Created > p.TimeCreated + PowerUp.PowerUpInvulnerabilityStartTime)
					p.Destroy();
			}
			return true;
		}

		public void Update()
		{
			TimeSpan timeBetweenExplosions = TimeSpan.FromSeconds(0.2f);

			//TODO: Expire _bodies

			if (Globals.GameTime.TotalGameTime - _lastExplosionCreatedTime > timeBetweenExplosions && _explosionsCreated < _explosionSize)
			{
				_lastExplosionCreatedTime = Globals.GameTime.TotalGameTime;

				_explosionsCreated++;

				//left
				if (_leftStillGoing)
				{
					if (!TryCreateExplosion(_centerX - _explosionsCreated, _centerY))
					{
						_leftStillGoing = false;
					}
				}

				if (_rightStillGoing)
				{
					if (!TryCreateExplosion(_centerX + _explosionsCreated, _centerY)) //right
					{
						_rightStillGoing = false;
					}
				}

				if (_upStillGoing)
				{
					if (!TryCreateExplosion(_centerX, _centerY - _explosionsCreated)) //up
					{
						_upStillGoing = false;
					}
				}

				if (_downStillGoing)
				{
					if (!TryCreateExplosion(_centerX, _centerY + _explosionsCreated))
					{
						_downStillGoing = false;
					}
				}

				//TODO: Create
			}

			foreach (var body in Bodies.ToArray())
			{
				if (body.Created + ExplosionBody.LifeTime < Globals.GameTime.TotalGameTime)
				{
					Globals.World.RemoveBody(body.Body);
					Bodies.Remove(body);
				}
			}
		}

		public bool IsFinished()
		{
			return Bodies.Count == 0;
		}

		public void Render()
		{
			foreach (var part in Bodies)
			{
				var p = (Globals.GameTime.TotalGameTime - part.Created).TotalSeconds / ExplosionBody.LifeTime.TotalSeconds;

				Draw(Resources.ExplosionAnim[Math.Min(21, (int)(p * Resources.ExplosionAnim.Length))], part.X, part.Y);
			}
		}

		private void Draw(Texture2D texture2D, int x, int y, Color? color = null)
		{
			Globals.SpriteBatch.Draw(texture2D, ConvertUnits.ToDisplayUnits(new Vector2(x, y) * GameWorld.CellSize), scale: new Vector2(Globals.TilePx / texture2D.Width) * 1.3f, origin: texture2D.CenteredOrigin(), color: color);
		}
	}
}
