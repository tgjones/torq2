using System;
using Microsoft.Xna.Framework;

namespace Torq2
{
	public struct IntVector2
	{
		public int X;
		public int Y;

		public static IntVector2 Zero
		{
			get { return new IntVector2(0, 0); }
		}

		/// <summary>
		/// Creates a new instance of IntVector2.
		/// </summary>
		/// <param name="x">Initial value for the x component of the vector.</param>
		/// <param name="y">Initial value for the y component of the vector.</param>
		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Creates a new instance of IntVector2.
		/// </summary>
		/// <param name="value">Value to initialise both components to.</param>
		public IntVector2(int value)
		{
			X = value;
			Y = value;
		}

		public override string ToString()
		{
			return "X: " + X + ", Y: " + Y;
		}

		#region Operators

		public static IntVector2 operator +(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.X + b.X, a.Y + b.Y);
		}

		public static IntVector2 operator -(IntVector2 a, IntVector2 b)
		{
			return new IntVector2(a.X - b.X, a.Y - b.Y);
		}

		public static IntVector2 operator *(IntVector2 a, int n)
		{
			return new IntVector2(a.X * n, a.Y * n);
		}

		public static IntVector2 operator /(IntVector2 a, int n)
		{
			return new IntVector2(a.X / n, a.Y / n);
		}

		public static bool operator ==(IntVector2 a, IntVector2 b)
		{
			return (a.X == b.X) && (a.Y == b.Y);
		}

		public static bool operator !=(IntVector2 a, IntVector2 b)
		{
			return !(a == b);
		}

		public static bool operator <=(IntVector2 a, IntVector2 b)
		{
			return (a.X <= b.X && a.Y <= b.Y);
		}

		public static bool operator >=(IntVector2 a, IntVector2 b)
		{
			return (a.X >= b.X && a.Y >= b.Y);
		}

		public static explicit operator IntVector2(Vector2 v)
		{
			return new IntVector2(Maths.FloorToInt(v.X), Maths.FloorToInt(v.Y));
		}

		public static implicit operator Vector2(IntVector2 v)
		{
			return new Vector2(v.X, v.Y);
		}

		public static IntVector2 operator %(IntVector2 a, int n)
		{
			return new IntVector2(a.X % n, a.Y % n);
		}

		#endregion
	}
}
