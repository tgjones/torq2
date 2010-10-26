using System;
using Microsoft.Xna.Framework;

namespace Torq2.Graphics.Cameras
{
	public interface ICameraService
	{
		Matrix ViewProjectionMatrix
		{
			get;
		}

		Matrix ViewMatrix
		{
			get;
		}

		Matrix ProjectionMatrix
		{
			get;
		}

		BoundingFrustum BoundingFrustum
		{
			get;
		}

		void Update(GameTime gameTime);
	}
}
