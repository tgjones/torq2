using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Torq2.Graphics;

namespace Torq2
{
	/// <summary>
	/// Simple class to calculate the frame-per-seconds rate of your game.
	/// </summary>
	public sealed partial class Framerate : DrawableGameComponent
	{
		#region Instance Fields

		private float deltaFPSTime;
		private double currentFramerate;
		private string windowTitle, displayFormat;
		private bool showDecimals;
		private SpriteBatch m_pSpriteBatch;
		private SpriteFont m_pTextFont;
		#endregion

		#region Instance Properties

		/// <summary>
		/// Gets the current framerate.
		/// </summary>
		/// <remarks>
		/// The 'Enabled' property must have been set to true to retrieve values greater than zero.
		/// </remarks>
		public double Current
		{
			get { return this.currentFramerate; }
		}

		/// <summary>
		/// Gets or Sets a value to enable framerate calculation.
		/// </summary>
		public new bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				base.Enabled = value;
				this.currentFramerate = 0;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the framerate will display decimals on screen or not.
		/// </summary>
		public bool ShowDecimals
		{
			get { return this.showDecimals; }
			set { this.showDecimals = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the decimal part of the framerate value must be display as fixed format (or as double format, otherwise).
		/// </summary>
		/// <remarks>
		/// The 'ShowDecimals' property must be set to true in order to set the proper format.
		/// </remarks>
		public bool FixedFormatDisplay
		{
			get { return this.displayFormat == "F"; }
			set { this.displayFormat = value == true ? "F" : "R"; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Parameterless constructor for this class.
		/// </summary>
		public Framerate(Game game) : base(game)
		{
			
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// Called after game initialization but before the first frame of the game.
		/// </summary>
		public override void Initialize()
		{
			this.currentFramerate = 0;
			this.windowTitle = String.Empty;
		}

		protected override void LoadContent()
		{
			m_pSpriteBatch = new SpriteBatch(GraphicsDevice);
			base.LoadContent();
		}

		/// <summary>
		/// Called when the gamecomponent needs to be updated.
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			if (this.Enabled)
			{
				// The time since Update() method was last called.
				float elapsed = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

				// Ads the elapsed time to the cumulative delta time.
				this.deltaFPSTime += elapsed;

				// If delta time is greater than a second: (a) the framerate is calculated, (b) it is marked to be drawn, and (c) the delta time is adjusted, accordingly.
				if (this.deltaFPSTime > 1000)
				{
					this.currentFramerate = 1000 / elapsed;
					this.deltaFPSTime -= 1000;
				}
			}
		}

		/// <summary>
		/// Called when the gamecomponent needs to be drawn.
		/// </summary>
		/// <remarks>
		/// Currently, the framerate is shown in the window's title of the game.
		/// </remarks>
		public override void Draw(GameTime gameTime)
		{
			// If the framerate can be drawn, it is shown in the window's title of the game.
			if (this.Visible)
			{
				string currentFramerateString = this.showDecimals ? this.currentFramerate.ToString(this.displayFormat) : ((int) this.currentFramerate).ToString("D");
				m_pSpriteBatch.DrawString(m_pTextFont, "FPS: " + currentFramerateString, new Vector2(10, 10), Color.White);
			}
		}

		#endregion
	}
}
