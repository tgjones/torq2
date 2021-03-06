using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.Terrain
{
	public class Sky
	{
		#region Fields

		private Model m_pSkyDomeModel;
		private Torq2Game m_pGame;

		private EffectWrapper m_pEffect;

		#endregion

		#region Constructor

		public Sky(Torq2Game pGame)
		{
			m_pGame = pGame;
		}

		#endregion

		#region Methods

		public void Create(GraphicsDevice pGraphicsDevice)
		{
			m_pSkyDomeModel = AssetLoader.LoadAsset<Model>(@"Models\SkyUnitDome", m_pGame, pGraphicsDevice);

			m_pEffect = new EffectWrapper(m_pGame, pGraphicsDevice, @"Effects\Sky");
		}

		public void Render(GraphicsDevice pGraphicsDevice)
		{
			ICameraService pCamera = (ICameraService) m_pGame.Services.GetService(typeof(ICameraService));

			float fScale = 20000.0f;
			Matrix tWorldMatrix = Matrix.CreateScale(fScale, fScale, -fScale) * Matrix.CreateTranslation(0.0f, 0.0f, -100.0f);
			Matrix tVP = pCamera.ViewMatrix * pCamera.ProjectionMatrix;
			m_pEffect.SetValue("World", Matrix.Transpose(tWorldMatrix));
			m_pEffect.SetValue("ViewProjection", Matrix.Transpose(tVP));

			Matrix[] tBoneTransforms = new Matrix[m_pSkyDomeModel.Bones.Count];
			m_pSkyDomeModel.CopyAbsoluteBoneTransformsTo(tBoneTransforms);

			m_pEffect.Render(new RenderCallback(RenderSky));
		}

		private void RenderSky(EffectWrapper pEffect, EffectPass pEffectPass)
		{
			ModelMeshPart meshPart = m_pSkyDomeModel.Meshes[0].MeshParts[0];

			pEffect.GraphicsDevice.Indices = meshPart.IndexBuffer;
			pEffect.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

			pEffect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				meshPart.VertexOffset,
				0,
				meshPart.NumVertices,
				meshPart.StartIndex,
				meshPart.PrimitiveCount);
		}

		#endregion
	}
}
