﻿using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ggj2015
{
	static class Extensions
	{
		public static Vector2 CenteredOrigin(this Texture2D texture)
		{
			return new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
		}

		public static Vector2 RotatedBy(this Vector2 vector, float angleRadian)
		{
			return Vector2.Transform(vector, Matrix.CreateRotationZ(angleRadian));
		}

		public static void DrawTile(this SpriteBatch batch, Texture2D tex, Vector2 pos, Color? color = null)
		{
			batch.Draw(tex, pos, scale: new Vector2(Globals.TilePx / tex.Width), origin: tex.CenteredOrigin(), color: color);
		}

		public static void DrawTileCell(this SpriteBatch batch, Texture2D tex, int x, int y, Color? color = null)
		{
			batch.DrawTile(tex, ConvertUnits.ToDisplayUnits(new Vector2(x, y) * GameWorld.CellSize), color);
		}

		public static void DrawStringCentered(this SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color color, float scale = 1)
		{
			var size = font.MeasureString(text) * scale;

			pos -= new Vector2(size.X / 2, 0);

			batch.DrawString(font, text, pos, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
	}
}
