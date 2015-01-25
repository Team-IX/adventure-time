using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ggj2015
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager _graphics;
		private DebugViewXNA _debugView;

		public Game1()
			: base()
		{
			_graphics = new GraphicsDeviceManager(this);
			Globals.Input = new InputManager(Services, Window.Handle);
			Components.Add(Globals.Input);

			Content.RootDirectory = "Content";

			_graphics.PreferredBackBufferWidth = Globals.RenderWidth;
			_graphics.PreferredBackBufferHeight = Globals.RenderHeight;
			_graphics.IsFullScreen = true;

			IsMouseVisible = true;

			Globals.GameWorld = new GameWorld();
			Globals.Controls = new SharedControlsManager();
			Globals.WebServer = new WebServer(Globals.Controls);
			Globals.Simulation = new Simulation();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();


#if true
			var form = (Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
			form.FormBorderStyle = FormBorderStyle.None;
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();

			form.Left = 0;
			form.Top = 0;
			form.Focus();
#endif

			// TODO: Add your initialization logic here

			Globals.World = new World(Vector2.Zero);
			_debugView = new DebugViewXNA(Globals.World);
			_debugView.LoadContent(GraphicsDevice, Content);
			_debugView.Flags = DebugViewFlags.Shape;
			//_debugView.Flags = (DebugViewFlags)0xff;

			Globals.Simulation.Reset();

			Globals.WebServer.Run();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			Resources.Load(Content);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Globals.GameTime = gameTime;
			//Looks like keyboards[4] is the one (windowmessage keyboard)
			var states = Globals.Input.Keyboards.Select(x => x.GetState()).ToArray();
			var gamePadState = Globals.Input.GamePads[0].GetState();
			if (gamePadState.Buttons.Back == ButtonState.Pressed || states.Any(s => s.IsKeyDown(Keys.Escape)))
			{
				Exit();
			}

			if (states.Any(s => s.IsKeyDown(Keys.Space)))
			{
				Globals.Simulation.Reset();
			}

			Globals.Controls.CheckForTimeouts();
			Globals.Simulation.UpdateControls();

			if (Globals.State == GameState.PlayingGame && Globals.Simulation.Players.Count(x => x.IsAlive) == 1)
			{
				Globals.State = GameState.PostGame;
				Globals.PostGame.Init();
			}

			if (Globals.State == GameState.PlayingGame)
			{
				if (!Globals.Simulation.IsSwapActive)
				{
					Globals.Simulation.Update();
					// TODO: Add your update logic here

					Globals.World.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
					Globals.Simulation.PostPhysicsUpdate();
				}
			}
			else if (Globals.State == GameState.PreGame)
			{
				Globals.PreGame.Update(states);
			}
			else if (Globals.State == GameState.PostGame)
			{
				Globals.PostGame.Update();
			}
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			Globals.WebServer.Stop();
			Globals.Controls.Stop();
			base.OnExiting(sender, args);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here

			var projection = Matrix.CreateOrthographicOffCenter(ConvertUnits.ToSimUnits(0 - ConvertUnits.LeftOffset), ConvertUnits.ToSimUnits(Globals.RenderWidth - ConvertUnits.LeftOffset), ConvertUnits.ToSimUnits(Globals.RenderHeight - ConvertUnits.TopOffset), ConvertUnits.ToSimUnits(0 - ConvertUnits.TopOffset), -1, 1);
			var scaleMatrix = Matrix.CreateScale((float)GraphicsDevice.PresentationParameters.BackBufferWidth / Globals.RenderWidth, (float)GraphicsDevice.PresentationParameters.BackBufferHeight / Globals.RenderHeight, 1);

			Globals.SpriteBatch.Begin(samplerState: SamplerState.AnisotropicWrap, transformMatrix: Matrix.CreateTranslation(ConvertUnits.LeftOffset - 70, ConvertUnits.TopOffset - 60, 0) * scaleMatrix);
			Globals.SpriteBatch.Draw(Resources.Objects.BackgroundTile, new Rectangle(0, 0, (int)((GameWorld.Width + 1) * Globals.TilePx), (int)((GameWorld.Height + 1) * Globals.TilePx)), new Rectangle(0, 0, (int)(Resources.Objects.BackgroundTile.Width * Globals.TilePx * GameWorld.CellSize * 0.5f), (int)(Resources.Objects.BackgroundTile.Height * Globals.TilePx* GameWorld.CellSize * 0.5f)), Color.White);
			Globals.SpriteBatch.End();

			Globals.SpriteBatch.Begin(transformMatrix: scaleMatrix);
			Globals.Simulation.Render();

			if (Globals.State == GameState.PreGame)
			{
				Globals.PreGame.Render();
			}
			else if (Globals.State == GameState.PostGame)
			{
				Globals.PostGame.Render();
			}
			Globals.SpriteBatch.End();

			//_debugView.RenderDebugData(projection, Matrix.Identity);

			base.Draw(gameTime);
		}
	}
}
