﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using ggj2015.GameObjects;
using Microsoft.Xna.Framework;

namespace ggj2015
{
	internal enum Tile
	{
		Empty,
		UnbreakableWall,
		BreakableWall
	}

	class GameWorld
	{
		public GameObject[,] ObjectsInCells;

		public const int Width = 17;
		public const int Height = 11;

		public const int BreakableWallCount = 100;

		public GameWorld()
		{
		}
		
		/*
			for (var y = 0; y < GameWorld.Height; y++)
			{
				string line = "";
				for (var x = 0; x < GameWorld.Width; x++)
				{
					switch (world.Tiles[x, y])
					{
						case Tile.BreakableWall:
							line += "!";
							break;
						case Tile.Empty:
							line += " ";
							break;
						case Tile.UnbreakableWall:
							line += "#";
							break;
					}
				}
				Console.WriteLine(line);
			}

*/

		public static float PlayerScale = 0.75f;
		public static float PlayerRadius = CellSize * PlayerScale * 0.4f;

		public static float UnbreakableWallRadius = CellSize * 0.3f;

		public void InitialPopulate()
		{
			ObjectsInCells = new GameObject[Width, Height];

			for (var x = 1; x < Width; x += 2)
			{
				for (var y = 1; y < Height; y += 2)
				{
					CreateUnbreakableWall(x, y);
				}
			}

			//Randomly place BreakableWall
			for (var i = 0; i < BreakableWallCount; i++)
			{
				var x = Globals.Random.Next(0, Width);
				var y = Globals.Random.Next(0, Height);

				if ((x <= 1 && y <= 1) ||
					(x <= 1 && y >= Height - 2) ||
					(x >= Width - 2 && y <= 1) ||
					(x >= Width - 2 && y >= Height - 2) ||
					ObjectsInCells[x, y] != null)
				{
					i--;
					continue;
				}

				CreateBreakableWall(x, y);
			}

			//Outside walls
			//Left
			BodyFactory.CreateRectangle(Globals.World, CellSize, (Height + 1) * CellSize, 0, new Vector2(- CellSize, (Height * 0.5f) * CellSize));
			//Right
			BodyFactory.CreateRectangle(Globals.World, CellSize, (Height + 1) * CellSize, 0, new Vector2((Width * CellSize), (Height * 0.5f) * CellSize - CellSize));

			//Top
			BodyFactory.CreateRectangle(Globals.World, (Width + 1) * CellSize, CellSize, 0, new Vector2((Width * 0.5f) * CellSize - CellSize, -CellSize));
			//Bottom
			BodyFactory.CreateRectangle(Globals.World, (Width + 1) * CellSize, CellSize, 0, new Vector2((Width * 0.5f) * CellSize, Height * CellSize));
		}

		private void CreateBreakableWall(int x, int y)
		{
			//return;
			var body = BodyFactory.CreateRectangle(Globals.World, CellSize, CellSize, 0, new Vector2(x * CellSize, y * CellSize));
			ObjectsInCells[x, y] = new BreakableWall(body);
		}

		public const float CellSize = 0.5f;

		private void CreateUnbreakableWall(int x, int y)
		{
			var body = BodyFactory.CreateRoundedRectangle(Globals.World, 
				CellSize, CellSize,
				UnbreakableWallRadius, UnbreakableWallRadius,
				2, 0, 
				new Vector2(x * CellSize, y * CellSize));
			body.Friction = 0;

			ObjectsInCells[x, y] = new UnbreakableWall(body);
			//BodyFactory.CreateRectangle(Globals.World, CellSize, CellSize, 0, new Vector2(x * CellSize, y * CellSize));
		}

		public void DestroyMaybe(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height)
				return;

			var o = ObjectsInCells[x, y];

			if (o == null || o is UnbreakableWall)
				return;

			if (o is BreakableWall)
			{
				var b = (BreakableWall)o;

				Globals.World.RemoveBody(b.Body);
				ObjectsInCells[x, y] = null;

				var powerUp = new PowerUp(x, y);
				Globals.Simulation.PowerUps.Add(powerUp);
			}
			else if (o is PowerUp)
			{
				var p = (PowerUp)o;

				p.Destroy();
			}
		}
	}
}
