using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2.Terrain
{
	/// <summary>
	/// Because the footprint (x, y) coordinates are local, these do not require
	/// 32-bit precision, so we pack them into two shorts, requiring only 4 bytes
	/// per vertex.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct TextureVertex
	{
		public Vector2 Position;

		public unsafe static int SizeInBytes
		{
			get { return sizeof(TextureVertex); }
		}

		public static readonly VertexElement[] VertexElements = new VertexElement[]
		{
			new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
		};

		public TextureVertex(Vector2 tPosition)
		{
			Position = tPosition;
		}

		public override string ToString()
		{
			return "Pos: " + Position;
		}
	}
}
