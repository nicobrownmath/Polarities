using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace Polarities
{
	public static class CustomCollision
	{
		//collision utilities copy-pasted from my random monogame project
		public static bool CheckAABBvPoint(Rectangle rectangle, float pointX, float pointY)
		{
			return pointX >= rectangle.X && pointX <= rectangle.Right && pointY >= rectangle.Y && pointY <= rectangle.Bottom;
		}

		public static bool CheckAABBvPoint(Rectangle rectangle, Vector2 point)
		{
			return point.X >= rectangle.X && point.X <= rectangle.Right && point.Y >= rectangle.Y && point.Y <= rectangle.Bottom;
		}

		public static bool CheckAABBvPoint(Vector2 position, Vector2 dimensions, Vector2 point)
		{
			return point.X >= position.X && point.X <= (position.X + dimensions.X) && point.Y >= position.Y && point.Y <= (position.Y + dimensions.Y);
		}

		public static bool CheckAABBvSegment(Rectangle rectangle, Vector2 start, Vector2 end)
		{
			if ((start.X < rectangle.Left && end.X < rectangle.Left) || (start.X > rectangle.Right && end.X > rectangle.Right) || (start.Y < rectangle.Top && end.Y < rectangle.Top) || (start.Y > rectangle.Bottom && end.Y > rectangle.Bottom))
				return false;

			float f1 = (end.Y - start.Y) * rectangle.Left + (start.X - end.X) * rectangle.Top + (end.X * start.Y - start.X * end.Y);
			float f2 = (end.Y - start.Y) * rectangle.Left + (start.X - end.X) * rectangle.Bottom + (end.X * start.Y - start.X * end.Y);
			float f3 = (end.Y - start.Y) * rectangle.Right + (start.X - end.X) * rectangle.Top + (end.X * start.Y - start.X * end.Y);
			float f4 = (end.Y - start.Y) * rectangle.Right + (start.X - end.X) * rectangle.Bottom + (end.X * start.Y - start.X * end.Y);

			if (f1 < 0 && f2 < 0 && f3 < 0 && f4 < 0) return false;
			if (f1 > 0 && f2 > 0 && f3 > 0 && f4 > 0) return false;

			return true;
		}

		public static bool CheckPointvTriangle(Vector2 point, Vector2 vertex0, Vector2 vertex1, Vector2 vertex2)
		{
			var s = vertex0.Y * vertex2.X - vertex0.X * vertex2.Y + (vertex2.Y - vertex0.Y) * point.X + (vertex0.X - vertex2.X) * point.Y;
			var t = vertex0.X * vertex1.Y - vertex0.Y * vertex1.X + (vertex0.Y - vertex1.Y) * point.X + (vertex1.X - vertex0.X) * point.Y;

			if ((s < 0) != (t < 0) || s == 0 || t == 0)
				return false;

			var A = -vertex1.Y * vertex2.X + vertex0.Y * (vertex2.X - vertex1.X) + vertex0.X * (vertex1.Y - vertex2.Y) + vertex1.X * vertex2.Y;

			return A < 0 ?
					(s < 0 && s + t > A) :
					(s > 0 && s + t < A);
		}
		public static bool CheckAABBvTriangle(Rectangle rectangle, Vector2 vertex0, Vector2 vertex1, Vector2 vertex2)
		{
			if (CheckAABBvPoint(rectangle, vertex0))
				return true;
			if (CheckAABBvPoint(rectangle, vertex1))
				return true;
			if (CheckAABBvPoint(rectangle, vertex2))
				return true;
			if (CheckAABBvSegment(rectangle, vertex0, vertex1))
				return true;
			if (CheckAABBvSegment(rectangle, vertex1, vertex2))
				return true;
			if (CheckAABBvSegment(rectangle, vertex2, vertex0))
				return true;
			if (CheckPointvTriangle(rectangle.TopLeft(), vertex0, vertex1, vertex2))
				return true;
			return false;
		}

		public static bool CheckAABBvDisc(Rectangle rectangle, Circle circle)
		{
			float nearestX = Math.Max(rectangle.X, Math.Min(circle.Center.X, rectangle.X + rectangle.Size().X));
			float nearestY = Math.Max(rectangle.Y, Math.Min(circle.Center.Y, rectangle.Y + rectangle.Size().Y));
			return (new Vector2(circle.Center.X - nearestX, circle.Center.Y - nearestY)).Length() < circle.radius;
		}
	}

	public readonly struct Circle
	{
		public readonly Vector2 Center;
		public readonly float radius;

		public Circle(Vector2 _Center, float _radius)
		{
			Center = _Center;
			radius = _radius;
		}
	}
}

