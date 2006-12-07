using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;

namespace Torq2.Terrain
{
	public class EdgeStitches : SceneObject
	{
		#region Fields

		private static VertexBuffer m_pSharedVertexBuffer;
		private static IndexBuffer m_pSharedIndexBuffer;

		private readonly int m_nGridSpacing;

		private Vector4 m_tScaleFactor;

		#endregion

		public EdgeStitches(int nGridSpacing)
		{
			m_nGridSpacing = nGridSpacing;
		}

		#region Start methods

		public override void Create(GraphicsDevice pGraphicsDevice)
		{
			// only create shared buffers once
			if (m_pSharedIndexBuffer == null)
				CreateSharedIndexBuffer(pGraphicsDevice);

			if (m_pSharedVertexBuffer == null)
				CreateSharedVertexBuffer(pGraphicsDevice);
		}

		/// <summary>
		/// Creates the single-instance index buffer that will be used by all ring fixups
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedIndexBuffer(GraphicsDevice pGraphicsDevice)
		{
			// create indices
			short[] pIndices = new short[Settings.EDGE_STITCHES_NUM_INDICES];

			short hIndex = 0;
			for (short i = 0; i < Settings.EDGE_STITCHES_NUM_INDICES; i++)
			{
				pIndices[i] = hIndex;
				if (i % Settings.GRID_SIZE_N != 0)
					++hIndex;
			}

			// create shared index buffer
			m_pSharedIndexBuffer = new IndexBuffer(
				pGraphicsDevice,
				typeof(short),
				Settings.EDGE_STITCHES_NUM_INDICES,
				ResourceUsage.None,
				ResourceManagementMode.Automatic);

			m_pSharedIndexBuffer.SetData<short>(pIndices);
		}

		/// <summary>
		/// Creates the vertex buffer which includes all four ring fixup vertices.
		/// There is a fixup on each side of the level, in the middle.
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedVertexBuffer(GraphicsDevice pGraphicsDevice)
		{
			// create vertices
			TerrainVertex.Shared[] pVertices = new TerrainVertex.Shared[Settings.EDGE_STITCHES_NUM_VERTICES];

			int nCounter = 0;

			// bottom side
			for (short x = 0; x < Settings.GRID_SIZE_N; x++)
			{
				pVertices[nCounter++] = new TerrainVertex.Shared(x, 0);
			}

			// right side
			for (short y = 1; y < Settings.GRID_SIZE_N; y++)
			{
				pVertices[nCounter++] = new TerrainVertex.Shared(Settings.GRID_SIZE_N_MINUS_ONE, y);
			}

			// top side
			for (short x = Settings.GRID_SIZE_N_MINUS_ONE - 1; x >= 0; x--)
			{
				pVertices[nCounter++] = new TerrainVertex.Shared(x, Settings.GRID_SIZE_N_MINUS_ONE);
			}

			// left side
			for (short y = Settings.GRID_SIZE_N_MINUS_ONE - 1; y >= 0; y--)
			{
				pVertices[nCounter++] = new TerrainVertex.Shared(0, y);
			}

			// create shared vertex buffer
			m_pSharedVertexBuffer = new VertexBuffer(
				pGraphicsDevice,
				typeof(TerrainVertex.Shared),
				Settings.EDGE_STITCHES_NUM_VERTICES,
				ResourceUsage.None,
				ResourceManagementMode.Automatic);

			m_pSharedVertexBuffer.SetData<TerrainVertex.Shared>(pVertices);
		}

		#endregion

		public void Update(Vector2 tLevelMinPosition)
		{
			Vector2 tWorldPosition = tLevelMinPosition;

			m_tScaleFactor = new Vector4(
				m_nGridSpacing,
				m_nGridSpacing,
				tWorldPosition.X,
				tWorldPosition.Y);
		}

		#region Draw methods

		public override void Render(EffectWrapper pEffect)
		{
			pEffect.GraphicsDevice.Vertices[0].SetSource(
				m_pSharedVertexBuffer,
				0,
				TerrainVertex.Shared.SizeInBytes);
			pEffect.GraphicsDevice.Indices = m_pSharedIndexBuffer;

			// set offset (scaling should be done in level)
			pEffect.SetValue("ScaleFactor", m_tScaleFactor);

			pEffect.Render(new RenderCallback(RenderEdgeStitches));
		}

		public void RenderEdgeStitches(EffectWrapper pEffect)
		{
			pEffect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
				0,                                      // base vertex
				0,                                      // min vertex index
				Settings.EDGE_STITCHES_NUM_VERTICES,    // total num vertices - note that is NOT just vertices that are indexed, but all vertices
				0,                                      // start index
				Settings.EDGE_STITCHES_NUM_PRIMITIVES); // primitive count
		}

		#endregion
	}
}
