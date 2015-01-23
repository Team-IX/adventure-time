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
	}
}
