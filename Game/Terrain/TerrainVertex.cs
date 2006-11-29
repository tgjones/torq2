using System;

namespace Torq2.Terrain
{
	public class TerrainVertex
	{
		/// <summary>
		/// Because the footprint (x, y) coordinates are local, these do not require
		/// 32-bit precision, so we pack them into two shorts, requiring only 4 bytes
		/// per vertex.
		/// </summary>
		public struct Shared
		{
			public short X;
			public short Y;

			public static int SizeInBytes
			{
				get { return sizeof(short) * 2; }
			}

			public Shared(short x, short y)
			{
				X = x;
				Y = y;
			}

			public override string ToString()
			{
				return "X: " + X + ", Y: " + Y;
			}
		}
	}
}
