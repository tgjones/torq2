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
	public struct TextureVertex : IVertexType
	{
		public Vector2 Position;

		/*public static int SizeInBytes
		{
			get { return sizeof(float) * 2; }
		}*/

		private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));

		public TextureVertex(Vector2 tPosition)
		{
			Position = tPosition;
		}

		public override string ToString()
		{
			return "Pos: " + Position;
		}

		public VertexDeclaration VertexDeclaration
		{
			get { return _vertexDeclaration; }
		}
	}
}
