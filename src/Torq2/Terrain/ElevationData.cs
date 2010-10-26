using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2.Terrain
{
	public class ElevationData
	{
		private Texture2D m_pHeightTexture;
		private ushort[] m_pHeightData;
		private int m_nWidth;
		private int m_nHeight;

		public Texture2D HeightMapTexture
		{
			get { return m_pHeightTexture; }
		}

		public ushort this[int x, int y]
		{
			get
			{
				if (x >= m_nWidth || x < 0 || y >= m_nHeight || y < 0)
				{
					return 0;
				}
				else
				{
					return m_pHeightData[(y * m_nWidth) + x];
				}
			}
		}

		public ElevationData(Game pGame, GraphicsDevice pGraphicsDevice, string sHeightMapFilename)
		{
			m_pHeightTexture = Importers.Tiff.TiffLoader.FromFile(pGraphicsDevice, sHeightMapFilename + ".tif");
			//m_pHeightTexture = Texture2D.FromFile(pGraphicsDevice, sHeightMapFilename + ".png");
			//m_pHeightTexture.Save("test.dds", ImageFileFormat.Dds);
			// load heightmap into array
			//m_pHeightTexture = AssetLoader.LoadAsset<Texture2D>(sHeightMapFilename, pGame, pGraphicsDevice);

			m_nWidth = m_pHeightTexture.Width;
			m_nHeight = m_pHeightTexture.Height;

			m_pHeightData = new ushort[m_nWidth * m_nHeight];
			m_pHeightTexture.GetData<ushort>(m_pHeightData);
		}
	}
}
