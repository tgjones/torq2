using System;
using Microsoft.Xna.Framework;

namespace Torq2.Graphics.Cameras
{
	public abstract class Camera : ICameraService
	{
		#region Fields

		private readonly Matrix m_tProjection;

		protected Matrix m_tView;

		#endregion

		#region Properties

		public Matrix ViewProjectionMatrix
		{
			get { return m_tView * m_tProjection; }
		}

		public Matrix ViewMatrix
		{
			get { return m_tView; }
		}

		public Matrix ProjectionMatrix
		{
			get { return m_tProjection; }
		}

		public BoundingFrustum BoundingFrustum
		{
			get { return new BoundingFrustum(this.ViewProjectionMatrix); }
		}

		#endregion

		#region Constructor

		public Camera()
		{
			const float PROJECTION_LEFT = -0.5f * Settings.SCREEN_RATIO;
			const float PROJECTION_RIGHT = 0.5f * Settings.SCREEN_RATIO;
			const float PROJECTION_BOTTOM = -0.5f;
			const float PROJECTION_TOP = 0.5f;
			const float PROJECTION_NEAR = 1.0f;
			const float PROJECTION_FAR = 30000.0f;

			m_tProjection = Matrix.CreatePerspectiveOffCenter(
				PROJECTION_LEFT,
				PROJECTION_RIGHT,
				PROJECTION_BOTTOM,
				PROJECTION_TOP,
				PROJECTION_NEAR,
				PROJECTION_FAR);
		}

		#endregion

		#region Methods

		public abstract void Update(GameTime gameTime);

		#endregion
	}
}
