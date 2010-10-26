using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Torq2
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Torq2Game : Game
	{
		private Microsoft.Xna.Framework.GraphicsDeviceManager graphics;
		private Torq2.Terrain.Terrain terrain;
		private Framerate framerate1;
		private Torq2.SimpleObjects.Vehicle cube1;
		private Torq2.Graphics.Cameras.ICameraService camera;

		public Torq2Game()
		{
			this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.terrain = new Torq2.Terrain.Terrain(this);
			this.framerate1 = new Torq2.Framerate(this);
			this.cube1 = new Torq2.SimpleObjects.Vehicle(this);
			this.camera = new Torq2.Graphics.Cameras.GodCamera(this);

			this.Components.Add(this.terrain);
			this.Components.Add(this.framerate1);
			this.Components.Add(this.cube1);

			this.Services.AddService(typeof(ContentManager), Content);
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

			using (Stream pFileStream = File.OpenWrite(sFileName))
				((Texture2D) graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget).SaveAsJpeg(pFileStream,
					graphics.GraphicsDevice.Viewport.Width,
					graphics.GraphicsDevice.Viewport.Height);
		}
	}
}