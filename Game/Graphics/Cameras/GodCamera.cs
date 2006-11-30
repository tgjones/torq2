using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Torq2.Graphics.Cameras
{
	/// <summary>
	/// Implements a first-person shooter style camera using the mouse for looking,
	/// and the keyboard (WASD) for translation
	/// </summary>
	public class GodCamera : Camera
	{
		#region Fields

		private const float ROTATION_SPEED = 0.0001f;
		private const float GAMEPAD_ROTATION_SPEED = ROTATION_SPEED * 10.0f;
		private const float TRANSLATION_SPEED = 0.025f;
		private const float GAMEPAD_TRANSLATION_SPEED = TRANSLATION_SPEED * 10.0f;
		private const float PITCH_CLAMP_ANGLE = MathHelper.PiOver2 - 0.1f;

		private readonly Vector3 m_tCameraReference;

		private Vector3 m_tPosition;
		private Vector3 m_tLookAt;
		private readonly Vector3 m_tUp;

		private float m_fPitch;
		private float m_fYaw;

		#endregion

		#region Constructor

		public GodCamera(Game pGame) : base()
		{
			m_tCameraReference = new Vector3(0, 1, 0);

			m_fPitch = 0;
			m_fYaw = 0;

			m_tPosition = new Vector3(10, 10, 140);
			m_tLookAt = new Vector3(0, 0, 0);
			m_tUp = new Vector3(0, 0, 1);

			ResetMousePosition();
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method implements a sort-of first-person-shooter camera,
		/// using these keys:
		/// W - Move forward (along line of sight)
		/// S - Move backward
		/// A - Strafe left
		/// D - Strafe right
		/// Q - Rotate left
		/// E - Rotate right
		/// R - Look up
		/// F - Look down
		/// T - Move up
		/// G - Move down
		/// </summary>
		/// <param name="fDeltaTime"></param>
		public override void Update(GameTime gameTime)
		{
			float fDeltaTime = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

			GamePadState pGamePadState = GamePad.GetState(PlayerIndex.One);

			#if TARGET_WINDOWS

			// get state from input devices
			KeyboardState pKeyboardState = Keyboard.GetState();
			MouseState pMouseState = Mouse.GetState();

			// translate mouse position into relative coordinates
			int nMouseMoveX = pMouseState.X - Settings.SCREEN_WIDTH_DIV_2;
			int nMouseMoveY = pMouseState.Y - Settings.SCREEN_HEIGHT_DIV_2;

			if (pMouseState.LeftButton == ButtonState.Released)
			{
				// look left and right
				m_fYaw -= nMouseMoveX * ROTATION_SPEED * fDeltaTime;

				// look up and down
				m_fPitch -= nMouseMoveY * ROTATION_SPEED * fDeltaTime;
			}

			#endif

			if (pGamePadState.IsConnected && pGamePadState.Buttons.LeftShoulder == ButtonState.Released)
			{
				// look left and right
				m_fYaw -= pGamePadState.ThumbSticks.Right.X * GAMEPAD_ROTATION_SPEED * fDeltaTime;

				// look up and down
				m_fPitch -= -pGamePadState.ThumbSticks.Right.Y * GAMEPAD_ROTATION_SPEED * fDeltaTime;
			}

			// clamp pitch to between vertically up and down
			m_fPitch = MathHelper.Clamp(m_fPitch, -PITCH_CLAMP_ANGLE, PITCH_CLAMP_ANGLE);

			// construct rotation matrix
			Matrix tYawMatrix = Matrix.CreateRotationZ(m_fYaw);
			Matrix tPitchMatrix = Matrix.CreateRotationX(m_fPitch);
			Matrix tRotationMatrix = tPitchMatrix * tYawMatrix;

			#if TARGET_WINDOWS

			// move forward and backward
			if (pKeyboardState.IsKeyDown(Keys.W) || pGamePadState.DPad.Up == ButtonState.Pressed)
			{
				Vector3 tMovement = new Vector3(0, TRANSLATION_SPEED, 0);
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}
			else if (pKeyboardState.IsKeyDown(Keys.S) || pGamePadState.DPad.Down == ButtonState.Pressed)
			{
				Vector3 tMovement = new Vector3(0, -TRANSLATION_SPEED, 0);
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}

			// use left button and mouse move to "zoom"
			if (pMouseState.LeftButton == ButtonState.Pressed)
			{
				Vector3 tMovement = new Vector3(0, TRANSLATION_SPEED, 0);
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition -= tMovement * nMouseMoveY * fDeltaTime;
			}

			// strafe left and right
			if (pKeyboardState.IsKeyDown(Keys.A) || pGamePadState.DPad.Left == ButtonState.Pressed)
			{
				Vector3 tMovement = new Vector3(TRANSLATION_SPEED, 0, 0);
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition -= tMovement * fDeltaTime;
			}
			else if (pKeyboardState.IsKeyDown(Keys.D) || pGamePadState.DPad.Right == ButtonState.Pressed)
			{
				Vector3 tMovement = new Vector3(-TRANSLATION_SPEED, 0, 0);
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition -= tMovement * fDeltaTime;
			}

			// move up and down
			if (pKeyboardState.IsKeyDown(Keys.T) || (pGamePadState.DPad.Up == ButtonState.Pressed && pGamePadState.Buttons.A == ButtonState.Pressed))
			{
				m_tPosition.Z += TRANSLATION_SPEED * fDeltaTime;
			}
			else if (pKeyboardState.IsKeyDown(Keys.G) || (pGamePadState.DPad.Down == ButtonState.Pressed && pGamePadState.Buttons.A == ButtonState.Pressed))
			{
				m_tPosition.Z -= TRANSLATION_SPEED * fDeltaTime;
			}

			#endif

			if (pGamePadState.IsConnected)
			{
				if (pGamePadState.Buttons.LeftShoulder == ButtonState.Released)
				{
					// strafe left and right
					Vector3 tMovement = new Vector3(GAMEPAD_TRANSLATION_SPEED, 0, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition -= -tMovement * pGamePadState.ThumbSticks.Left.X * fDeltaTime;

					// move forward and backward
					tMovement = new Vector3(0, GAMEPAD_TRANSLATION_SPEED, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition += tMovement * pGamePadState.ThumbSticks.Left.Y * fDeltaTime;
				}

				// use left shoulder button and right stick to "zoom"
				if (pGamePadState.Buttons.LeftShoulder == ButtonState.Pressed)
				{
					Vector3 tMovement = new Vector3(0, GAMEPAD_TRANSLATION_SPEED * 10.0f, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition += tMovement * pGamePadState.ThumbSticks.Right.Y * fDeltaTime;
				}
			}

			// calculate the direction vector for the camera
			Vector3 tTransformedReference = Vector3.Transform(m_tCameraReference, tRotationMatrix);
			m_tLookAt = m_tPosition + tTransformedReference;

			m_tView = Matrix.CreateLookAt(m_tPosition, m_tLookAt, m_tUp);

			#if TARGET_WINDOWS

			ResetMousePosition();

			#endif
		}

		#if TARGET_WINDOWS

		/// <summary>
		/// Resets the mouse cursor to the centre of the window
		/// </summary>
		private void ResetMousePosition()
		{
			Mouse.SetPosition(Settings.SCREEN_WIDTH_DIV_2, Settings.SCREEN_HEIGHT_DIV_2);
		}

		#endif

		#endregion
	}
}
