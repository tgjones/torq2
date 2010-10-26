using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.Terrain
{
	public class Level
	{
		#region Fields

		private readonly int m_nGridSpacing;
		private readonly int m_nHalfWidth;

		private IntVector2 m_tPositionMin;

		private Terrain m_pParentTerrain;

		private Block[] m_pBlocks;
		private RingFixups m_pRingFixups;
		private InteriorTrim m_pInteriorTrim;
		private Block[] m_pCentreBlocks;
		private InteriorTrim m_pCentreInteriorTrim;
		private EdgeStitches m_pEdgeStitches;

		private Level m_pNextFinerLevel;
		private bool m_bFinestLevel;
		private Level m_pNextCoarserLevel;

		private bool m_bActive;

		private IntVector2 m_tToroidalOrigin;

		private RenderTarget2D m_pElevationTexture;
		private RenderTarget2D m_pNormalMapTexture;

		private EffectWrapper m_pNormalMapUpdateEffect;
		private EffectWrapper m_pElevationUpdateEffect;

		#endregion

		#region Properties

		public IntVector2 PositionMin
		{
			get { return m_tPositionMin; }
		}

		public IntVector2 PositionMax
		{
			get
			{
				return m_tPositionMin + new IntVector2(Settings.GRID_SIZE_N_MINUS_ONE * m_nGridSpacing);
			}
		}

		private bool Active
		{
			get { return m_bActive; }
		}

		public Texture2D ElevationTexture
		{
			get { return m_pElevationTexture; }
		}

		public Texture2D NormalMapTexture
		{
			get { return m_pNormalMapTexture; }
		}

		private InteriorTrim InteriorTrim
		{
			get { return m_pInteriorTrim; }
		}

		#endregion

		#region Constructor

		public Level(Terrain pParentTerrain, int nGridSpacing)
		{
			m_pParentTerrain = pParentTerrain;

			m_nGridSpacing = nGridSpacing;
			m_nHalfWidth = (Settings.GRID_SIZE_N_MINUS_ONE / 2) * m_nGridSpacing;

			// comment these offsets!
			m_pBlocks = new Block[12];
			m_pBlocks[0] = CreateBlock(new Vector2(0, 0));
			m_pBlocks[1] = CreateBlock(new Vector2(0, Settings.BLOCK_SIZE_M_MINUS_ONE));
			m_pBlocks[2] = CreateBlock(new Vector2(0, (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2));
			m_pBlocks[3] = CreateBlock(new Vector2(0, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[4] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE, 0));
			m_pBlocks[5] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[6] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2, 0));
			m_pBlocks[7] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[8] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, 0));
			m_pBlocks[9] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, Settings.BLOCK_SIZE_M_MINUS_ONE));
			m_pBlocks[10] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2));
			m_pBlocks[11] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));

			m_pCentreBlocks = new Block[4];
			m_pCentreBlocks[0] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M, Settings.BLOCK_SIZE_M));
			m_pCentreBlocks[1] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M, (Settings.BLOCK_SIZE_M * 2) - 1));
			m_pCentreBlocks[2] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M * 2) - 1, Settings.BLOCK_SIZE_M));
			m_pCentreBlocks[3] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M * 2) - 1, (Settings.BLOCK_SIZE_M * 2) - 1));

			m_pRingFixups = new RingFixups(m_nGridSpacing);

			m_pInteriorTrim = new InteriorTrim(m_nGridSpacing);

			m_pCentreInteriorTrim = new InteriorTrim(m_nGridSpacing);

			m_pEdgeStitches = new EdgeStitches(m_nGridSpacing);

			m_bActive = true;

			//m_tToroidalOrigin = new IntVector2(1, 1);
			m_tToroidalOrigin = IntVector2.Zero;
		}

		#endregion

		#region Methods

		private Block CreateBlock(Vector2 tGridSpaceOffset)
		{
			return new Block(m_nGridSpacing, tGridSpaceOffset, tGridSpaceOffset * m_nGridSpacing);
		}

		public void Create(GraphicsDevice pGraphicsDevice, Level pNextFinerLevel, Level pNextCoarserLevel, IntVector2 tViewerPosition2D)
		{
			foreach (Block pBlock in m_pBlocks)
			{
				pBlock.Create(pGraphicsDevice);
			}

			m_pRingFixups.Create(pGraphicsDevice);

			m_pInteriorTrim.Create(pGraphicsDevice);

			foreach (Block pBlock in m_pCentreBlocks)
			{
				pBlock.Create(pGraphicsDevice);
			}

			m_pCentreInteriorTrim.Create(pGraphicsDevice);

			m_pEdgeStitches.Create(pGraphicsDevice);

			m_pNextFinerLevel = pNextFinerLevel;
			m_pNextCoarserLevel = pNextCoarserLevel;

			// set initial min position of level
			IntVector2 tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;
			IntVector2 tDeltaPositionTemp1 = tViewerPosGridCoords - Settings.CentralSquareMin;
			IntVector2 tDeltaPositionTemp2 = new IntVector2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
			IntVector2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
			m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

			m_pElevationTexture = new RenderTarget2D(pGraphicsDevice,
				Settings.ELEVATION_TEXTURE_SIZE,
				Settings.ELEVATION_TEXTURE_SIZE,
				false,
				SurfaceFormat.Vector4,
				DepthFormat.Depth24);

			m_pNormalMapTexture = new RenderTarget2D(pGraphicsDevice,
				Settings.NORMAL_MAP_TEXTURE_SIZE,
				Settings.NORMAL_MAP_TEXTURE_SIZE,
				false,
				SurfaceFormat.HalfVector2,
				DepthFormat.Depth24);

			m_pNormalMapUpdateEffect = new EffectWrapper(m_pParentTerrain.ParentGame, pGraphicsDevice,
				@"Content\Effects\ComputeNormals");

			m_pElevationUpdateEffect = new EffectWrapper(m_pParentTerrain.ParentGame, pGraphicsDevice,
				@"Content\Effects\UpdateElevation");
		}

		public void Create2(GraphicsDevice pGraphicsDevice)
		{
			UpdateElevationTexture(pGraphicsDevice);
			UpdateNormalMapTexture(pGraphicsDevice);
		}

		public void Update(Vector3 tViewerPosition, IntVector2 tViewerPosition2D, GraphicsDevice pGraphicsDevice)
		{
			// there is a central square of 2x2 grid units that we use to determine
			// if the level needs to move. if it doesn't need to move, we still might
			// need to update the interior trim position

			// each level only ever moves by TWICE the level's grid spacing

			// transform viewer position from world coords to grid coords
			IntVector2 tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

			// check if viewer position is still in central square
			bool lUpdate = false;
			if (!(tViewerPosGridCoords >= Settings.CentralSquareMin && tViewerPosGridCoords <= Settings.CentralSquareMax))
			{
				// need to move level, so calculate new minimum position
				IntVector2 tDeltaPositionTemp1 = tViewerPosGridCoords - Settings.CentralSquareMin;
				IntVector2 tDeltaPositionTemp2 = new IntVector2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
				IntVector2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
				m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

				// recalculate viewer pos in grid coordinates
				tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

				lUpdate = true;
			}

			Vector2 tPositionMin = m_tPositionMin;

			foreach (Block pBlock in m_pBlocks)
			{
				pBlock.Update(tPositionMin);
			}

			m_pRingFixups.Update(tPositionMin);

			m_pInteriorTrim.Update(tPositionMin, tViewerPosGridCoords);

			m_pEdgeStitches.Update(tPositionMin);

			// if this is the finest active level, we need to fill the hole with more blocks
			if (m_pNextFinerLevel == null || !m_pNextFinerLevel.Active)
			{
				m_bFinestLevel = true;

				m_pInteriorTrim.ActiveInteriorTrim = InteriorTrim.WhichInteriorTrim.BottomLeft;

				foreach (Block pBlock in m_pCentreBlocks)
				{
					pBlock.Update(tPositionMin);
				}

				m_pCentreInteriorTrim.Update(tPositionMin, tViewerPosGridCoords);
				m_pCentreInteriorTrim.ActiveInteriorTrim = InteriorTrim.WhichInteriorTrim.TopRight;
			}
			else
			{
				m_bFinestLevel = false;
			}

			// checking blocks for frustum visibility
			ICameraService pCamera = (ICameraService) this.m_pParentTerrain.ParentGame.Services.GetService(typeof(ICameraService));
			BoundingFrustum lCameraFrustum = pCamera.BoundingFrustum;
			foreach (Block pBlock in m_pBlocks)
			{
				//pBlock.Visible = true;
				pBlock.Visible = (lCameraFrustum.Contains(pBlock.BoundingBox) != ContainmentType.Disjoint);
			}
			foreach (Block pBlock in m_pCentreBlocks)
			{
				//pBlock.Visible = true;
				pBlock.Visible = (lCameraFrustum.Contains(pBlock.BoundingBox) != ContainmentType.Disjoint);
			}

			if (lUpdate)
			{
				UpdateElevationTexture(pGraphicsDevice);
				UpdateNormalMapTexture(pGraphicsDevice);
			}
		}

		#region Update elevation texture methods

		private void UpdateElevationTexture(GraphicsDevice pGraphicsDevice)
		{
			// TODO: currently we just update the whole texture. we need to do this toroidally
			RenderTarget2D pSavedSurface = (RenderTarget2D) pGraphicsDevice.GetRenderTargets()[0].RenderTarget;

			pGraphicsDevice.SetRenderTarget(m_pElevationTexture);

			Matrix tWorld = Matrix.Identity;
			Matrix tView = Matrix.Identity;
			Matrix tProjection = Matrix.CreateOrthographicOffCenter(0, Settings.ELEVATION_TEXTURE_SIZE,
				Settings.ELEVATION_TEXTURE_SIZE, 0, 0.0f, 1.0f);

			m_pElevationUpdateEffect.SetValue("WorldViewProjection", Matrix.Transpose(tProjection));

			// render to texture here
			m_pElevationUpdateEffect.SetValue("HeightMapTexture", m_pParentTerrain.HeightMapTexture);
			m_pElevationUpdateEffect.SetValue("HeightMapSizeInverse", 1.0f / (float) m_pParentTerrain.HeightMapTexture.Width);
			m_pElevationUpdateEffect.SetValue("GridSpacing", (float) m_nGridSpacing);
			m_pElevationUpdateEffect.SetValue("WorldPosMin", (Vector2) m_tPositionMin);
			m_pElevationUpdateEffect.SetValue("ToroidalOrigin", m_tToroidalOrigin);
			m_pElevationUpdateEffect.SetValue("ElevationTextureSize", Settings.ELEVATION_TEXTURE_SIZE);
			m_pElevationUpdateEffect.Render(new RenderCallback(RenderElevationTexture));

			pGraphicsDevice.SetRenderTarget(pSavedSurface);

			//m_pElevationTexture.GetTexture().Save("Level " + m_nGridSpacing + ".dds", ImageFileFormat.Dds);
		}

		private void RenderElevationTexture(EffectWrapper pEffect)
		{
			float fMinVertexX = 0.0f;
			float fMaxVertexX = Settings.ELEVATION_TEXTURE_SIZE;
			float fMinVertexY = Settings.ELEVATION_TEXTURE_SIZE;
			float fMaxVertexY = 0.0f;

			TextureVertex[] pVertices = new TextureVertex[4];

			// (-1, -1)
			pVertices[0] = new TextureVertex(new Vector2(fMinVertexX, fMinVertexY));

			// (-1, 1)
			pVertices[1] = new TextureVertex(new Vector2(fMinVertexX, fMaxVertexY));

			// (1, -1)
			pVertices[2] = new TextureVertex(new Vector2(fMaxVertexX, fMinVertexY));

			// (1, 1)
			pVertices[3] = new TextureVertex(new Vector2(fMaxVertexX, fMaxVertexY));

			pEffect.GraphicsDevice.DrawUserPrimitives<TextureVertex>(
				PrimitiveType.TriangleStrip,
				pVertices, 0, 2);
		}

		#endregion

		#region Update normal map texture methods

		private void UpdateNormalMapTexture(GraphicsDevice pGraphicsDevice)
		{
			RenderTargetBinding[] pSavedSurfaces = pGraphicsDevice.GetRenderTargets();

			pGraphicsDevice.SetRenderTarget(m_pNormalMapTexture);

			pGraphicsDevice.Clear(Color.Black);

			Matrix tWorld = Matrix.Identity;
			Matrix tView = Matrix.Identity;
			Matrix tProjection = Matrix.CreateOrthographicOffCenter(0, Settings.NORMAL_MAP_TEXTURE_SIZE,
				Settings.NORMAL_MAP_TEXTURE_SIZE, 0, 0.0f, 1.0f);

			m_pNormalMapUpdateEffect.SetValue("WorldViewProjection", Matrix.Transpose(tProjection));

			// render to texture here
			m_pNormalMapUpdateEffect.SetValue("ElevationTexture", m_pElevationTexture);
			m_pNormalMapUpdateEffect.SetValue("ElevationTextureSizeInverse", Settings.ELEVATION_TEXTURE_SIZE_INVERSE);
			m_pNormalMapUpdateEffect.SetValue("NormalScaleFactor", new Vector2(0.5f / (float)m_nGridSpacing));
			m_pNormalMapUpdateEffect.Render(new RenderCallback(RenderNormalMapTexture));

			pGraphicsDevice.SetRenderTargets(pSavedSurfaces);

			//m_pNormalMapTexture.GetTexture().Save("NormalMap " + m_nGridSpacing + ".dds", ImageFileFormat.Dds);
		}

		private void RenderNormalMapTexture(EffectWrapper pEffect)
		{
			float fMinVertexX = 0.0f;
			float fMaxVertexX = Settings.NORMAL_MAP_TEXTURE_SIZE - 1;
			float fMinVertexY = fMaxVertexX;
			float fMaxVertexY = 0.0f;

			TextureVertex[] pVertices = new TextureVertex[4];

			// (-1, -1)
			pVertices[0] = new TextureVertex(new Vector2(fMinVertexX, fMinVertexY));

			// (-1, 1)
			pVertices[1] = new TextureVertex(new Vector2(fMinVertexX, fMaxVertexY));

			// (1, -1)
			pVertices[2] = new TextureVertex(new Vector2(fMaxVertexX, fMinVertexY));

			// (1, 1)
			pVertices[3] = new TextureVertex(new Vector2(fMaxVertexX, fMaxVertexY));

			pEffect.GraphicsDevice.DrawUserPrimitives<TextureVertex>(
				PrimitiveType.TriangleStrip,
				pVertices, 0, 2);
		}

		#endregion

		#region Render methods

		public void Render(EffectWrapper pEffect)
		{
			// apply vertex texture
			pEffect.SetValue("ElevationTexture", m_pElevationTexture);

			pEffect.SetValue("ViewerPos", (m_pParentTerrain.Viewer.Position2D - m_tPositionMin) / m_nGridSpacing);
			pEffect.SetValue("AlphaOffset", Settings.AlphaOffset);
			pEffect.SetValue("OneOverWidth", Settings.TransitionWidthInverse);
			pEffect.SetValue("GridSize", Settings.GRID_SIZE_N);

			pEffect.SetValue("GrassTexture", m_pParentTerrain.GrassTexture);

			pEffect.SetValue("NormalMapTexture", m_pNormalMapTexture);

			Vector2 tCoarserGridPosMin = new Vector2();
			if (m_pNextCoarserLevel != null)
				tCoarserGridPosMin = m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin;
			pEffect.SetValue("CoarserNormalMapTextureOffset", tCoarserGridPosMin);

			pEffect.SetValue("CoarserNormalMapTexture", (m_pNextCoarserLevel != null) ? m_pNextCoarserLevel.NormalMapTexture : null);
			pEffect.SetValue("NormalMapTextureSizeInverse", Settings.NORMAL_MAP_TEXTURE_SIZE_INVERSE);
			pEffect.SetValue("NormalMapTextureSize", Settings.NORMAL_MAP_TEXTURE_SIZE);

			pEffect.SetValue("LightDirection", Vector3.Normalize(new Vector3(0.0f, 0.0f, 1)));

			Vector2 tToroidalOriginScaled = (Vector2) m_tToroidalOrigin / (float) Settings.ELEVATION_TEXTURE_SIZE;
			Vector2 tGridSizeScaled = new Vector2(((float) Settings.GRID_SIZE_N / (float) Settings.ELEVATION_TEXTURE_SIZE));
			pEffect.SetValue("ToroidalOffsets", new Vector4(tToroidalOriginScaled, tGridSizeScaled.X, tGridSizeScaled.Y));
			pEffect.SetValue("ElevationTextureSize", Settings.ELEVATION_TEXTURE_SIZE);

			#region Render blocks

			pEffect.SetValue("Shading", new Vector4(0.7f, 0.0f, 0.0f, 1.0f));

			// we set the vertices and indices here because they are the same for all blocks
			pEffect.GraphicsDevice.SetVertexBuffer(Block.SharedVertexBuffer);
			pEffect.GraphicsDevice.Indices = Block.SharedIndexBuffer;

			pEffect.Render(new RenderCallback(RenderBlocks));

			#endregion

			#region Render ring fix-ups

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(0.0f, 0.7f, 0.0f, 1.0f));

			m_pRingFixups.Render(pEffect);

			#endregion

			#region Render interior trim

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(0.0f, 0.0f, 0.7f, 1.0f));

			m_pInteriorTrim.Render(pEffect);

			#endregion

			#region Render centre blocks for finest level

			if (m_bFinestLevel)
			{
				pEffect.SetValue("Shading", new Vector4(0.7f, 0.7f, 0.0f, 1.0f));

				// we set the vertices and indices here because they are the same for all blocks
				pEffect.GraphicsDevice.SetVertexBuffer(Block.SharedVertexBuffer);
				pEffect.GraphicsDevice.Indices = Block.SharedIndexBuffer;

				pEffect.Render(new RenderCallback(RenderCentreBlocks));

				pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
				pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
				pEffect.SetValue("Shading", new Vector4(0.0f, 0.0f, 0.7f, 1.0f));

				m_pCentreInteriorTrim.Render(pEffect);
			}

			#endregion

			#region Render edge stitches

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(0.7f, 0.7f, 0.0f, 1.0f));

			m_pEdgeStitches.Render(pEffect);

			#endregion
		}

		public void RenderBlocks(EffectWrapper pEffect)
		{
			foreach (Block pBlock in m_pBlocks)
			{
				pBlock.Render(pEffect);
			}
		}

		public void RenderCentreBlocks(EffectWrapper pEffect)
		{
			foreach (Block pBlock in m_pCentreBlocks)
			{
				pBlock.Render(pEffect);
			}
		}

		#endregion

		#endregion
	}
}
