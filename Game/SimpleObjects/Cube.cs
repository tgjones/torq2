using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.SimpleObjects
{
	public class Cube : DrawableGameComponent, ITerrainViewer
	{
		private const float TRANSLATION_SPEED = 5f;

		private VertexPositionColor[] nonIndexedCube;
		private BasicEffect effect;

		private Vector3 m_tPosition;

		public Vector3 Position
		{
			get { return m_tPosition; }
		}

		public Vector2 Position2D
		{
			get { return new Vector2(m_tPosition.X, m_tPosition.Y); }
		}

		public Cube(Game game) : base(game)
		{
			m_tPosition = new Vector3(0, 0, 0);
		}

		public override void Initialize()
		{
			nonIndexedCube = new VertexPositionColor[36];

			Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, -1.0f);
			Vector3 bottomLeftFront = new Vector3(-1.0f, -1.0f, -1.0f);
			Vector3 topRightFront = new Vector3(1.0f, 1.0f, -1.0f);
			Vector3 bottomRightFront = new Vector3(1.0f, -1.0f, -1.0f);
			Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, 1.0f);
			Vector3 topRightBack = new Vector3(1.0f, 1.0f, 1.0f);
			Vector3 bottomLeftBack = new Vector3(-1.0f, -1.0f, 1.0f);
			Vector3 bottomRightBack = new Vector3(1.0f, -1.0f, 1.0f);

			// Front face
			nonIndexedCube[0] =
					new VertexPositionColor(topLeftFront, Color.Red);
			nonIndexedCube[1] =
					new VertexPositionColor(bottomLeftFront, Color.Red);
			nonIndexedCube[2] =
					new VertexPositionColor(topRightFront, Color.Red);
			nonIndexedCube[3] =
					new VertexPositionColor(bottomLeftFront, Color.Red);
			nonIndexedCube[4] =
					new VertexPositionColor(bottomRightFront, Color.Red);
			nonIndexedCube[5] =
					new VertexPositionColor(topRightFront, Color.Red);

			// Back face 
			nonIndexedCube[6] =
					new VertexPositionColor(topLeftBack, Color.Orange);
			nonIndexedCube[7] =
					new VertexPositionColor(topRightBack, Color.Orange);
			nonIndexedCube[8] =
					new VertexPositionColor(bottomLeftBack, Color.Orange);
			nonIndexedCube[9] =
					new VertexPositionColor(bottomLeftBack, Color.Orange);
			nonIndexedCube[10] =
					new VertexPositionColor(topRightBack, Color.Orange);
			nonIndexedCube[11] =
					new VertexPositionColor(bottomRightBack, Color.Orange);

			// Top face
			nonIndexedCube[12] =
					new VertexPositionColor(topLeftFront, Color.Yellow);
			nonIndexedCube[13] =
					new VertexPositionColor(topRightBack, Color.Yellow);
			nonIndexedCube[14] =
					new VertexPositionColor(topLeftBack, Color.Yellow);
			nonIndexedCube[15] =
					new VertexPositionColor(topLeftFront, Color.Yellow);
			nonIndexedCube[16] =
					new VertexPositionColor(topRightFront, Color.Yellow);
			nonIndexedCube[17] =
					new VertexPositionColor(topRightBack, Color.Yellow);

			// Bottom face 
			nonIndexedCube[18] =
					new VertexPositionColor(bottomLeftFront, Color.Purple);
			nonIndexedCube[19] =
					new VertexPositionColor(bottomLeftBack, Color.Purple);
			nonIndexedCube[20] =
					new VertexPositionColor(bottomRightBack, Color.Purple);
			nonIndexedCube[21] =
					new VertexPositionColor(bottomLeftFront, Color.Purple);
			nonIndexedCube[22] =
					new VertexPositionColor(bottomRightBack, Color.Purple);
			nonIndexedCube[23] =
					new VertexPositionColor(bottomRightFront, Color.Purple);

			// Left face
			nonIndexedCube[24] =
					new VertexPositionColor(topLeftFront, Color.Blue);
			nonIndexedCube[25] =
					new VertexPositionColor(bottomLeftBack, Color.Blue);
			nonIndexedCube[26] =
					new VertexPositionColor(bottomLeftFront, Color.Blue);
			nonIndexedCube[27] =
					new VertexPositionColor(topLeftBack, Color.Blue);
			nonIndexedCube[28] =
					new VertexPositionColor(bottomLeftBack, Color.Blue);
			nonIndexedCube[29] =
					new VertexPositionColor(topLeftFront, Color.Blue);

			// Right face 
			nonIndexedCube[30] =
					new VertexPositionColor(topRightFront, Color.Green);
			nonIndexedCube[31] =
					new VertexPositionColor(bottomRightFront, Color.Green);
			nonIndexedCube[32] =
					new VertexPositionColor(bottomRightBack, Color.Green);
			nonIndexedCube[33] =
					new VertexPositionColor(topRightBack, Color.Green);
			nonIndexedCube[34] =
					new VertexPositionColor(topRightFront, Color.Green);
			nonIndexedCube[35] =
					new VertexPositionColor(bottomRightBack, Color.Green);

			GraphicsDevice pGraphicsDevice = ((IGraphicsDeviceService) this.Game.Services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
			effect = new BasicEffect(pGraphicsDevice, null);
		}

		public override void Update(GameTime gameTime)
		{
			effect.World = Matrix.CreateTranslation(m_tPosition);
			
			ICameraService pCamera = (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
			effect.View = pCamera.ViewMatrix;
			effect.Projection = pCamera.ProjectionMatrix;

			float fDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

			KeyboardState pKeyboardState = Keyboard.GetState();

			if (pKeyboardState.IsKeyDown(Keys.Left))
			{
				m_tPosition.X -= TRANSLATION_SPEED * fDeltaTime;
			}
			else if (pKeyboardState.IsKeyDown(Keys.Right))
			{
				m_tPosition.X += TRANSLATION_SPEED * fDeltaTime;
			}

			if (pKeyboardState.IsKeyDown(Keys.Up))
			{
				m_tPosition.Y += TRANSLATION_SPEED * fDeltaTime;
			}
			else if (pKeyboardState.IsKeyDown(Keys.Down))
			{
				m_tPosition.Y -= TRANSLATION_SPEED * fDeltaTime;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			/*GraphicsDevice pGraphicsDevice = ((IGraphicsDeviceService) this.Game.Services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			using (VertexDeclaration decl = new VertexDeclaration(pGraphicsDevice, VertexPositionColor.VertexElements))
			{
				pGraphicsDevice.VertexDeclaration = decl;

				//This code would go between a device BeginScene-EndScene block.
				effect.Begin();
				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Begin();

					// nonIndexedCube is a VertexBuffer derived in the application
					pGraphicsDevice.DrawUserPrimitives<VertexPositionColor>
															(PrimitiveType.TriangleList, nonIndexedCube, 0, 12);
					effect.CommitChanges();
					pass.End();
				}
				effect.End();

				//pGraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
			}*/
		}
	}
}
