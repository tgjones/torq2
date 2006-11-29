using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.Terrain
{
	public class Terrain : DrawableGameComponent
	{
		private BitmapFont m_pTextWriter;
		private static VertexElement[] m_pVertexElements;

		private EffectWrapper m_pEffect;

		private GraphicsDevice m_pGraphicsDevice;

		private Level[] m_pLevels;
		private IntVector2 m_tPreviousViewerPosition;

		private ITerrainViewer m_pViewer;

		private ElevationData m_pElevationData;

		private SpriteBatch m_pLevelHeightMap;

		public EffectWrapper Effect
		{
			get { return m_pEffect; }
		}

		public ITerrainViewer Viewer
		{
			get { return m_pViewer; }
			set { m_pViewer = value; }
		}

		public Texture2D HeightMapTexture
		{
			get { return m_pElevationData.HeightMapTexture; }
		}

		public Game ParentGame
		{
			get { return this.Game; }
		}

		static Terrain()
		{
			// set vertex definition
			m_pVertexElements = new VertexElement[]
			{
				new VertexElement(0, 0, VertexElementFormat.Short2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
			};
		}

		public Terrain(Game game) : base(game)
		{
			const int NUM_LEVELS = 11;
			m_pLevels = new Level[NUM_LEVELS];
			int nCounter = 0;
			for (int i = NUM_LEVELS - 1; i >= 0; i--)
			{
				m_pLevels[nCounter++] = new Level(this, Maths.Pow(2, i));
			}
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			m_pGraphicsDevice = this.GraphicsDevice;

			VertexDeclaration pVertexDeclaration = new VertexDeclaration(m_pGraphicsDevice, m_pVertexElements);

			m_pEffect = new EffectWrapper(this.Game, m_pGraphicsDevice,
				@"Content\Effects\Terrain",
				pVertexDeclaration);

			m_pElevationData = new ElevationData(this.Game, m_pGraphicsDevice, @"Content\Terrains\Terragen 1");

			for (int i = 0, length = m_pLevels.Length; i < length; i++)
			{
				Level pLevel = m_pLevels[i];

				Level pNextFinerLevel = null, pNextCoarserLevel = null;
				if (i != length - 1)
					pNextFinerLevel = m_pLevels[i + 1];
				if (i > 0)
					pNextCoarserLevel = m_pLevels[i - 1];

				pLevel.Create(m_pGraphicsDevice, pNextFinerLevel, pNextCoarserLevel, (IntVector2) m_pViewer.Position2D);
			}

			for (int i = 0, length = m_pLevels.Length; i < length; i++)
			{
				Level pLevel = m_pLevels[i];
				pLevel.Create2(m_pGraphicsDevice);
			}

			m_pTextWriter = new BitmapFont("Content/Fonts/LucidaConsole10.xml");
			m_pTextWriter.Reset(this.GraphicsDevice);

			m_pLevelHeightMap = new SpriteBatch(m_pGraphicsDevice);

			base.LoadGraphicsContent(loadAllContent);
		}

		public int GetHeight(int x, int y)
		{
			return m_pElevationData[x, y];
		}

		public override void Update(GameTime gameTime)
		{
			IntVector2 tViewerPosition2D = (IntVector2) m_pViewer.Position2D;
			IntVector2 tDeltaViewerPosition = tViewerPosition2D - m_tPreviousViewerPosition;
			m_tPreviousViewerPosition = tViewerPosition2D;

			// only bother updating levels if viewer has moved at all
			/*if (tDeltaViewerPosition != IntVector2.Zero)
			{*/
				// update levels in coarse-to-fine order
				foreach (Level pLevel in m_pLevels)
				{
					pLevel.Update(m_pViewer.Position, tViewerPosition2D, m_pGraphicsDevice);
				}
			//}

			ICameraService pCamera = (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
			m_pEffect.SetValue("WorldViewProjection", Matrix.Transpose(pCamera.ViewProjectionMatrix));
		}

		public override void Draw(GameTime gameTime)
		{
			m_pGraphicsDevice.RenderState.DepthBufferEnable = true;
			foreach (Level pLevel in m_pLevels)
			{
				pLevel.Render(m_pEffect);
			}
		}
	}
}
