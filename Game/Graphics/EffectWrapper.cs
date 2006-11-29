using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2.Graphics
{
	public delegate void RenderCallback(EffectWrapper pEffect);

	/// <summary>
	/// Summary description for EffectInstance.
	/// </summary>
	public class EffectWrapper
	{
		#region Variables

		private GraphicsDevice m_pGraphicsDevice;

		private Effect m_pEffect;
		private Hashtable m_pEffectHandles;
		private VertexDeclaration m_pVertexDeclaration;

		#endregion

		#region Properties

		public GraphicsDevice GraphicsDevice
		{
			get { return m_pGraphicsDevice; }
		}

		#endregion

		#region Constructor

		public EffectWrapper(Game pGame, GraphicsDevice pGraphicsDevice, string sFilename,
			VertexDeclaration pVertexDeclaration)
		{
			m_pGraphicsDevice = pGraphicsDevice;
			m_pVertexDeclaration = pVertexDeclaration;

			// load effect from file
			m_pEffect = AssetLoader.LoadAsset<Effect>(sFilename, pGame, m_pGraphicsDevice);

			// set technique
			m_pEffect.CurrentTechnique = m_pEffect.Techniques[0];

			// cache effect handles
			m_pEffectHandles = new Hashtable();
		}

		#endregion

		#region Methods

		#region Set value methods

		private EffectParameter GetEffectHandle(string sParameterName)
		{
			if (m_pEffectHandles.Contains(sParameterName))
			{
				return (EffectParameter) m_pEffectHandles[sParameterName];
			}
			else
			{
				EffectParameter pEffectHandle = m_pEffect.Parameters[sParameterName];
				m_pEffectHandles.Add(sParameterName, pEffectHandle);
				return pEffectHandle;
			}
		}

		public void SetValue(string sParameterName, Matrix tMatrix)
		{
			GetEffectHandle(sParameterName).SetValue(tMatrix);
		}

		public void SetValue(string sParameterName, Vector2 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Vector3 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Vector4 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Texture pTexture)
		{
			GetEffectHandle(sParameterName).SetValue(pTexture);
		}

		public void SetValue(string sParameterName, float fValue)
		{
			GetEffectHandle(sParameterName).SetValue(fValue);
		}

		#endregion

		public void CommitChanges()
		{
			m_pEffect.CommitChanges();
		}

		public void Render(RenderCallback pRenderCallback)
		{
			// set vertex declaration
			m_pGraphicsDevice.VertexDeclaration = m_pVertexDeclaration;

			// begin effect
			m_pEffect.Begin(SaveStateMode.SaveState);

			// loop through all of the effect's passes
			foreach (EffectPass pEffectPass in m_pEffect.CurrentTechnique.Passes)
			{
				// set state for the current effect pass
				pEffectPass.Begin();

				// render
				pRenderCallback(this);

				// end the effect pass
				pEffectPass.End();
			}

			// must call End to signal the end of the technique
			m_pEffect.End();
		}

		#endregion
	}
}