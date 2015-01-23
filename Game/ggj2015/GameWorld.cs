using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Factories;
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
		public Tile[,] Tiles;

		public GameObject[,] ObjectsInCells;

		public const int Width = 17;
		public const int Height = 11;

		public const int BreakableWallCount = 100;

		public GameWorld()
		{
			Tiles = new Tile[Width, Height];

			for (var x = 1; x < Width; x += 2)
			{
				for (var y = 1; y < Height; y += 2)
				{
					Tiles[x, y] = Tile.UnbreakableWall;
				}
			}

			//Randomly place BreakableWall

			var random = new Random();

			for (var i = 0; i < BreakableWallCount; i++)
			{
				var x = random.Next(0, Width);
				var y = random.Next(0, Height);

				if ((x <= 1 && y <= 1) ||
					(x <= 1 && y >= Height - 2) ||
					(x >= Width - 2 && y <= 1) ||
					(x >= Width - 2 && y >= Height - 2) ||
					Tiles[x, y] != Tile.Empty)
				{
					i--;
					continue;
				}

				Tiles[x, y] = Tile.BreakableWall;
			}
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

		public void InitialPopulate()
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					switch (Tiles[x, y])
					{
						case Tile.BreakableWall:
							CreateBreakableWall(x, y);
							break;
						case Tile.Empty:
							break;
						case Tile.UnbreakableWall:
							CreateUnbreakableWall(x, y);
							break;
						default:
							throw new Exception();
					}
				}
			}

			//Outside walls
			//Left
			BodyFactory.CreateRectangle(Globals.World, CellSize, (Height + 1) * CellSize, 0, new Vector2(LeftOffset - CellSize * 0.5f, BottomOffset + (Height * 0.5f) * CellSize));
			//Right
			BodyFactory.CreateRectangle(Globals.World, CellSize, (Height + 1) * CellSize, 0, new Vector2(LeftOffset + (Width * CellSize)  + CellSize * 0.5f, BottomOffset + (Height * 0.5f) * CellSize));

			//Top
			BodyFactory.CreateRectangle(Globals.World, (Width + 1) * CellSize, CellSize, 0, new Vector2(LeftOffset + (Width * 0.5f) * CellSize, BottomOffset - CellSize * 0.5f));
			//Bottom
			BodyFactory.CreateRectangle(Globals.World, (Width + 1) * CellSize, CellSize, 0, new Vector2(LeftOffset + (Width * 0.5f) * CellSize, BottomOffset + Height * CellSize + CellSize * 0.5f));
		}

		const int LeftOffset = 1;
		const float BottomOffset = 0.2f;

		private void CreateBreakableWall(int x, int y)
		{
			BodyFactory.CreateRectangle(Globals.World, CellSize, CellSize, 0, new Vector2(LeftOffset + (x + 0.5f) * CellSize, BottomOffset + (y + 0.5f) * CellSize));
		}

		private const float CellSize = 0.6f;

		private void CreateUnbreakableWall(int x, int y)
		{
			BodyFactory.CreateRectangle(Globals.World, CellSize, CellSize, 0, new Vector2(LeftOffset + (x + 0.5f) * CellSize, BottomOffset + (y + 0.5f) * CellSize));
		}
	}
}
