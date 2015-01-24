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
	class Explosion : GameObject
	{
		private readonly int _centerX;
		private readonly int _centerY;
		private readonly int _minX;
		private readonly int _maxX;
		private readonly int _minY;
		private readonly int _maxY;

		//from base public Body Body { get; set; }
		public Body Body2 { get; set; }

		private const float Padding = GameWorld.CellSize * 0.2f;

		public TimeSpan TimeToDie { get; set; }
		public TimeSpan TimeCreated { get; set; }


		public Explosion(int centerX, int centerY, int minX, int maxX, int minY, int maxY)
		{
			_centerX = centerX;
			_centerY = centerY;
			_minX = minX;
			_maxX = maxX;
			_minY = minY;
			_maxY = maxY;


			TimeCreated = Globals.GameTime.TotalGameTime;
			TimeToDie = Globals.GameTime.TotalGameTime + TimeSpan.FromSeconds(0.3f);

			//				((Player)p.UserData).Die();
			float midX = ((minX + maxX) / 2f) * GameWorld.CellSize;
			float midY = ((minY + maxY) / 2f) * GameWorld.CellSize;
			var centerForX = new Vector2(midX, centerY * GameWorld.CellSize);
			var centerForY = new Vector2(centerX * GameWorld.CellSize, midY);

			Body = BodyFactory.CreateRectangle(Globals.World, (maxX - minX + 1) * GameWorld.CellSize - Padding, GameWorld.CellSize - Padding, 0, centerForX, 0, BodyType.Dynamic, this);
			Body.SleepingAllowed = false;
			Body.IsSensor = true;
			Body.OnCollision += BodyOnOnCollision;

			Body2 = BodyFactory.CreateRectangle(Globals.World, GameWorld.CellSize - Padding, (maxY - minY + 1) * GameWorld.CellSize - Padding, 0, centerForY, 0, BodyType.Dynamic, this);
			Body2.SleepingAllowed = false;
			Body2.IsSensor = true;
			Body2.OnCollision += BodyOnOnCollision;
		}


		private bool BodyOnOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			var otherBody = (fixtureA.Body == Body || fixtureA.Body == Body2) ? fixtureB.Body : fixtureA.Body;

			if (otherBody.UserData is Player)
				((Player)otherBody.UserData).Die();
			if (otherBody.UserData is Bomb)
				((Bomb)otherBody.UserData).ForceExplode();
			if (otherBody.UserData is PowerUp)
			{
				var p = (PowerUp)otherBody.UserData;
				if (TimeCreated > p.TimeCreated + PowerUp.PowerUpInvulnerabilityStartTime)
					p.Destroy();
			}
			return true;
		}

		public bool IsFinished()
		{
			if (Globals.GameTime.TotalGameTime < TimeToDie)
				return false;


			Globals.World.RemoveBody(Body);
			Globals.World.RemoveBody(Body2);

			return true;
		}

		public void Render()
		{
			Draw(Resources.Explosion.Mid, _centerX, _centerY);

			//Left-Right Left
			for (var x = _minX + 1; x < _centerX; x++)
				Draw(Resources.Explosion.LeftRight, x, _centerY);
			//Left-Right Right
			for (var x = _centerX + 1; x < _maxX; x++)
				Draw(Resources.Explosion.LeftRight, x, _centerY);
			//Left cap
			if (_minX < _centerX)
				Draw(Resources.Explosion.Left, _minX, _centerY);
			//Left cap
			if (_maxX > _centerX)
				Draw(Resources.Explosion.Right, _maxX, _centerY);

			//UP-Down up
			for (var y = _minY + 1; y < _centerY; y++)
				Draw(Resources.Explosion.UpDown, _centerX, y);
			//updown down
			for (var y = _centerY + 1; y < _maxY; y++)
				Draw(Resources.Explosion.UpDown, _centerX, y);
			//up cap
			if (_minY < _centerY)
				Draw(Resources.Explosion.Up, _centerX, _minY);
			//down cap
			if (_maxY > _centerY)
				Draw(Resources.Explosion.Down, _centerX, _maxY);

			
			//bafasdasdewatwet
		}

		private void Draw(Texture2D texture2D, int x, int y)
		{
			Globals.SpriteBatch.DrawTileCell(texture2D, x, y);
		}
	}
}
