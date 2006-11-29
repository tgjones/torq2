using System;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;

namespace Torq2
{
	public abstract class SceneObject
	{
		public abstract void Create(GraphicsDevice pGraphicsDevice);
		public abstract void Render(EffectWrapper pEffect);
	}
}
