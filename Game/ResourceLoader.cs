using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2
{
	/// <summary>
	/// Summary description for ResourceLoader.
	/// </summary>
	public static class AssetLoader
	{
		public static T LoadAsset<T>(string sAssetName, Game pGame, GraphicsDevice pDevice)
		{
			ContentManager pContentManager = (ContentManager) pGame.Services.GetService(typeof(ContentManager));
			T pAsset = pContentManager.Load<T>(sAssetName);
			return pAsset;
		}
	}
}