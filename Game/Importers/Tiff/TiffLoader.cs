using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2.Importers.Tiff
{
	public class TiffLoader
	{
		public static Texture2D FromFile(GraphicsDevice pGraphicsDevice, string sFileName)
		{
			TiffFile pTiffFile = new TiffFile(sFileName);

			Texture2D pTexture = new Texture2D(pGraphicsDevice, pTiffFile.Width, pTiffFile.Height,
				1, ResourceUsage.None, SurfaceFormat.Luminance16, ResourceManagementMode.Automatic);

			pTexture.SetData<ushort>(pTiffFile.ImageData);
			
			return pTexture;
		}
	}
}
