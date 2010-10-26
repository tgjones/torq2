using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torq2.Graphics;
using Torq2.Graphics.Cameras;

namespace Torq2.SimpleObjects
{
    #region Data Structures

    internal class Wheel
    {
        public Vector3 Position;
        public Vector3 Orientation;
        public Wheel(Vector3 position)
        {
            Position = position;
        }
    }

    internal class Axle
    {
        public float FullLockWheelOrientation;
        public Wheel[] Wheels;
        public Axle(int numberOfWheels)
        {
            Wheels = new Wheel[numberOfWheels];
            FullLockWheelOrientation = 0.0f;
        }
    }

    internal class Engine
    {
        public float Torque;
    }

    #endregion

    public class Vehicle : DrawableGameComponent, ITerrainViewer
    {
        #region Fields

        private Model m_pBodyModel;
        private Model m_pWheelModel;
        private Torq2Game m_pGame;

        private Axle[] m_pAxles;
        private Engine m_pEngine;
        private float m_fTotalMass;
		private float m_fSteeringWheelOrientation;
        private Vector3 m_tPosition;
        private Vector3 m_tVelocity;
        
        #endregion

        #region Properties

        public Vector3 Position
		{
			get { return m_tPosition; }
		}

		public Vector2 Position2D
		{
			get { return new Vector2(m_tPosition.X, m_tPosition.Y); }
        }

        #endregion

        #region Constructor

        public Vehicle(Torq2Game pGame) : base(pGame)
		{
            m_pGame = pGame;
        }

        #endregion

        public override void Initialize()
		{
            // Constants listed here will need to be externalised...
            const float TYRE_RADIUS = 0.3865f;
            const float WHEELBASE = 3.09626f;
            const float FRONT_TRACK = 1.4859f;
            const float REAR_TRACK = 1.4859f;
            const float MAX_TORQUE = 10000.0f; // Gearing required
            const float TOTAL_MASS = 1243.0f;

            // Initialise physics
            m_fTotalMass = TOTAL_MASS;
            m_pEngine = new Engine();
            m_pEngine.Torque = MAX_TORQUE;
            m_pAxles = new Axle[2];
            m_pAxles[0] = new Axle(2);
            m_pAxles[1] = new Axle(2);
            m_pAxles[0].FullLockWheelOrientation = 1.0f;
            m_pAxles[0].Wheels[0] = new Wheel(new Vector3(-FRONT_TRACK / 2, 1.3f, TYRE_RADIUS));
            m_pAxles[0].Wheels[1] = new Wheel(new Vector3(FRONT_TRACK / 2, 1.3f, TYRE_RADIUS));
            m_pAxles[1].Wheels[0] = new Wheel(new Vector3(-REAR_TRACK / 2, 1.3f - WHEELBASE, TYRE_RADIUS));
            m_pAxles[1].Wheels[1] = new Wheel(new Vector3(REAR_TRACK / 2, 1.3f - WHEELBASE, TYRE_RADIUS));
            m_fSteeringWheelOrientation = 0.0f;
            m_tPosition = new Vector3(0, 0, 4);
            m_tVelocity = new Vector3(0, 0, 0);

            // Initialise graphics
            m_pBodyModel = AssetLoader.LoadAsset<Model>(@"Models\ToyotaBody", m_pGame, m_pGame.GraphicsDevice);
            m_pWheelModel = AssetLoader.LoadAsset<Model>(@"Models\ToyotaWheel", m_pGame, m_pGame.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
		{
            KeyboardState pKeyboardState = Keyboard.GetState();
            float fDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply engine torque
            Vector3 tAcceleration = new Vector3(0, 0, 0);
            if (pKeyboardState.IsKeyDown(Keys.Up)) tAcceleration.Y += m_pEngine.Torque / m_fTotalMass;
			if (pKeyboardState.IsKeyDown(Keys.Down)) tAcceleration.Y -= m_pEngine.Torque / m_fTotalMass;

            // Set the steering wheel position (simulates analogue controls)
            float fAnalogueSteeringPosition = 0.0f;
            if (pKeyboardState.IsKeyDown(Keys.Left)) fAnalogueSteeringPosition -= 1.0f;
            if (pKeyboardState.IsKeyDown(Keys.Right)) fAnalogueSteeringPosition += 1.0f;
            if (fAnalogueSteeringPosition > m_fSteeringWheelOrientation)
                m_fSteeringWheelOrientation += Math.Min(
                    fAnalogueSteeringPosition - m_fSteeringWheelOrientation,
                    (fAnalogueSteeringPosition - m_fSteeringWheelOrientation) * 5.0f * fDeltaTime);
            else
                m_fSteeringWheelOrientation -= Math.Min(
                    m_fSteeringWheelOrientation - fAnalogueSteeringPosition,
                    (m_fSteeringWheelOrientation - fAnalogueSteeringPosition) * 5.0f * fDeltaTime);
            
			// Update the steering roadwheel(s) orientation
            for (int i = 0; i < m_pAxles.Length; i++)
                if (m_pAxles[i].FullLockWheelOrientation != 0.0f)
                    for (int j = 0; j < m_pAxles[i].Wheels.Length; j++)
                        m_pAxles[i].Wheels[j].Orientation.Z = m_fSteeringWheelOrientation;

            // Update the position from the velocity
            m_tVelocity += tAcceleration * fDeltaTime;
            m_tPosition += m_tVelocity * fDeltaTime;
		}

		public override void Draw(GameTime gameTime)
		{	
			//This code would go between a device BeginScene-EndScene block.
			GraphicsDevice pGraphicsDevice = ((IGraphicsDeviceService) this.Game.Services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
		    
            // Draw the vehicle body
            foreach (ModelMesh mesh in m_pBodyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    ICameraService pCamera = (ICameraService)this.Game.Services.GetService(typeof(ICameraService));
                    effect.View = pCamera.ViewMatrix;
                    effect.Projection = pCamera.ProjectionMatrix;
                    effect.World = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(m_tPosition);                    
                    effect.EnableDefaultLighting();
                    pGraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                    mesh.Draw();
                }
            }

            // Draw the wheels
            for (int i = 0; i < m_pAxles.Length; i++)
            {
                for (int j = 0; j < m_pAxles[i].Wheels.Length; j++)
                {
                    foreach (ModelMesh mesh in m_pWheelModel.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            ICameraService pCamera = (ICameraService)this.Game.Services.GetService(typeof(ICameraService));
                            effect.View = pCamera.ViewMatrix;
                            effect.Projection = pCamera.ProjectionMatrix;
                            effect.World =
                                Matrix.CreateScale(m_pAxles[i].Wheels[j].Position.X > 0.0f ? -1.0f : 1.0f, 1.0f, 1.0f) *
                                Matrix.CreateRotationX(MathHelper.PiOver2) *
                                Matrix.CreateRotationZ(-m_pAxles[i].Wheels[j].Orientation.Z) *
                                Matrix.CreateTranslation(m_tPosition + m_pAxles[i].Wheels[j].Position);

                            effect.EnableDefaultLighting();
														pGraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                            mesh.Draw();
                        }
                    }
                }
            }
        }
	}
}
