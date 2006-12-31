using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torq2.Video;

namespace Torq2
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Torq2Game : Microsoft.Xna.Framework.Game
	{
		private Microsoft.Xna.Framework.GraphicsDeviceManager graphics;
		private Microsoft.Xna.Framework.Content.ContentManager content;
		private Torq2.Terrain.Terrain terrain;
		private Framerate framerate1;
		private Torq2.SimpleObjects.Vehicle cube1;
		private Torq2.Graphics.Cameras.ICameraService camera;

		private bool m_bRecordingVideo = false;
		private AviWriter m_pAviWriter;

        public GraphicsDevice GraphicsDevice
        {
            get { return graphics.GraphicsDevice; }
        }

		public Torq2Game()
		{
			this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
			this.content = new ContentManager(Services);
			this.terrain = new Torq2.Terrain.Terrain(this);
			this.framerate1 = new Torq2.Framerate(this);
			this.cube1 = new Torq2.SimpleObjects.Vehicle(this);
			this.camera = new Torq2.Graphics.Cameras.GodCamera(this);

			this.Components.Add(this.terrain);
			this.Components.Add(this.framerate1);
			this.Components.Add(this.cube1);

			this.Services.AddService(typeof(ContentManager), content);
			this.Services.AddService(typeof(Torq2.Graphics.Cameras.ICameraService), camera);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			this.graphics.PreferredBackBufferHeight = Settings.SCREEN_WIDTH;
			this.graphics.PreferredBackBufferWidth = Settings.SCREEN_HEIGHT;
			this.graphics.SynchronizeWithVerticalRetrace = false;
			this.graphics.PreferMultiSampling = false;

			this.framerate1.Enabled = true;
			this.framerate1.FixedFormatDisplay = false;
			this.framerate1.ShowDecimals = false;
			this.framerate1.Visible = true;

			this.IsFixedTimeStep = false;

			terrain.Viewer = cube1;

			base.Initialize();
		}

		/// <summary>
		/// Load your graphics content.  If loadAllContent is true, you should
		/// load content from both ResourceManagementMode pools.  Otherwise, just
		/// load ResourceManagementMode.Manual content.
		/// </summary>
		/// <param name="loadAllContent">Which type of content to load.</param>
		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			if (loadAllContent)
			{
				// TODO: Load any ResourceManagementMode.Automatic content
			}

			// TODO: Load any ResourceManagementMode.Manual content
		}


		/// <summary>
		/// Unload your graphics content.  If unloadAllContent is true, you should
		/// unload content from both ResourceManagementMode pools.  Otherwise, just
		/// unload ResourceManagementMode.Manual content.  Manual content will get
		/// Disposed by the GraphicsDevice during a Reset.
		/// </summary>
		/// <param name="unloadAllContent">Which type of content to unload.</param>
		protected override void UnloadGraphicsContent(bool unloadAllContent)
		{
			if (unloadAllContent == true)
			{
				content.Unload();
			}
		}

		protected override void Update(GameTime gameTime)
		{
			camera.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.SkyBlue);

			base.Draw(gameTime);

			KeyboardState pKeyboardState = Keyboard.GetState();
			if (pKeyboardState.IsKeyDown(Keys.F11))
			{
				TakeScreenshot();
			}

			if (pKeyboardState.IsKeyDown(Keys.F9))
			{
				m_bRecordingVideo = true;
				if (m_pAviWriter != null) m_pAviWriter.Close();
				CreateVideo();
			}
			else if (pKeyboardState.IsKeyDown(Keys.F10))
			{
				m_bRecordingVideo = false;
			}

			if (m_bRecordingVideo)
			{
				AddVideoFrame();
			}
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			base.OnExiting(sender, args);
			if (m_pAviWriter != null) m_pAviWriter.Close();
		}

		private void TakeScreenshot()
		{
			if (!Directory.Exists("Screenshots"))
			{
				Directory.CreateDirectory("Screenshots");
			}

			// get next index
			string sFileName;
			int i = 1;
			do
			{
				sFileName = @"Screenshots\Screen" + i.ToString().PadLeft(4, '0') + ".jpg";
				i++;
			}
			while (File.Exists(sFileName));

			using (Texture2D dstTexture = new Texture2D(
										graphics.GraphicsDevice,
										graphics.GraphicsDevice.Viewport.Width,
										graphics.GraphicsDevice.Viewport.Height,
										1,
										ResourceUsage.ResolveTarget,
										SurfaceFormat.Color,
										ResourceManagementMode.Manual))
			{
				graphics.GraphicsDevice.ResolveBackBuffer(dstTexture);

				dstTexture.Save(sFileName, ImageFileFormat.Jpg);
			}
		}

		private void AddVideoFrame()
		{
			using (Texture2D dstTexture = new Texture2D(
										graphics.GraphicsDevice,
										graphics.GraphicsDevice.Viewport.Width,
										graphics.GraphicsDevice.Viewport.Height,
										1,
										ResourceUsage.ResolveTarget,
										SurfaceFormat.Color,
										ResourceManagementMode.Manual))
			{
				graphics.GraphicsDevice.ResolveBackBuffer(dstTexture);
				m_pAviWriter.AddFrame(dstTexture);
			}
		}

		private void CreateVideo()
		{
			if (!Directory.Exists("Videos"))
			{
				Directory.CreateDirectory("Videos");
			}

			// get next index
			string sFileName;
			int i = 1;
			do
			{
				sFileName = @"Videos\Video" + i.ToString().PadLeft(4, '0') + ".avi";
				i++;
			}
			while (File.Exists(sFileName));

			m_pAviWriter = new AviWriter();
			m_pAviWriter.Open(sFileName, 25);
		}
	}
}