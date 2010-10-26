using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.Terrain
{
	public class Terrain : DrawableGameComponent
	{
		private SpriteFont m_pTextFont;

		private EffectWrapper m_pEffect;

		private GraphicsDevice m_pGraphicsDevice;

		private Level[] m_pLevels;
		private Sky m_pSky;

		private IntVector2 m_tPreviousViewerPosition;

		private ITerrainViewer m_pViewer;

		private ElevationData m_pElevationData;

		private SpriteBatch m_pLevelHeightMap;

		private Texture2D m_pGrassTexture;

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

		public Texture2D GrassTexture
		{
			get { return m_pGrassTexture; }
		}

		public Terrain(Game pGame)
			: base(pGame)
		{
			const int NUM_LEVELS = 5;
			m_pLevels = new Level[NUM_LEVELS];
			int nCounter = 0;
			for (int i = NUM_LEVELS - 1; i >= 0; i--)
			{
				m_pLevels[nCounter++] = new Level(this, Maths.Pow(2, i));
			}

			m_pSky = new Sky((Torq2Game) pGame);
		}

		protected override void LoadContent()
		{
			m_pGraphicsDevice = this.GraphicsDevice;

			m_pEffect = new EffectWrapper(this.Game, m_pGraphicsDevice,
				@"Effects\Terrain");

			m_pElevationData = new ElevationData(this.Game, m_pGraphicsDevice, @"Content\Terrains\SimpleLoop");

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

			foreach (Level pLevel in m_pLevels)
			{
				pLevel.Create2(m_pGraphicsDevice);
			}

			m_pSky.Create(m_pGraphicsDevice);

			m_pTextFont = Game.Content.Load<SpriteFont>(@"Fonts\LucidaConsole");

			m_pLevelHeightMap = new SpriteBatch(m_pGraphicsDevice);

			m_pGrassTexture = Game.Content.Load<Texture2D>(@"Terrains\Grass");

			base.LoadContent();
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
			m_pGraphicsDevice.DepthStencilState = DepthStencilState.Default;
			foreach (Level pLevel in m_pLevels)
			{
				pLevel.Render(m_pEffect);
			}

			m_pSky.Render(m_pGraphicsDevice);

			//m_pLevelHeightMap.Begin();
			//m_pLevelHeightMap.Draw(m_pLevels[0].ElevationTexture, Vector2.Zero, Color.White);
			//m_pLevelHeightMap.End();
		}
	}
}
