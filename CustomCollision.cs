using Microsoft.Xna.Framework;
using System;
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

    //line such that ax+by+c=0
    public readonly struct Line
    {
        public readonly float a;
        public readonly float b;
        public readonly float c;

        public Line(float _a, float _b, float _c)
        {
            a = _a;
            b = _b;
            c = _c;
        }

        public static Line AngledFrom(Vector2 point, float rotation)
        {
            return new Line((float)Math.Sin(rotation), (float)-Math.Cos(rotation), (float)(point.Y * Math.Cos(rotation) - point.X * Math.Sin(rotation)));
        }

        public static Line LineThrough(Vector2 point1, Vector2 point2)
        {
            return new Line((point2 - point1).Y, (point1 - point2).X, (point1 - point2).Y * point1.X + (point2 - point1).X * point1.Y);
        }

        public static Line Bisector(Vector2 point1, Vector2 point2)
        {
            return AngledFrom((point1 + point2) / 2, (point1 - point2).ToRotation() + MathHelper.PiOver2);
        }

        public Vector2 Intersection(Line line2)
        {
            float denominator = line2.a * b - a * line2.b;
            return new Vector2((line2.b * c - b * line2.c) / denominator, (a * line2.c - line2.a * c) / denominator);
        }
    }
}

