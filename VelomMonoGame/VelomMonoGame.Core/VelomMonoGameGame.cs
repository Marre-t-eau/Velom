using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using VelomMonoGame.Core.Localization;
using VelomMonoGame.Core.Sources.Pages;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public class VelomMonoGameGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;

        internal IPage Page { get; set; }

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public VelomMonoGameGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            if (IsDesktop)
            {
                IsMouseVisible = true;
                Window.AllowUserResizing = true;
                Window.ClientSizeChanged += Window_ClientSizeChanged;
            }
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Page.Size = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            /*
            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);*/

            TextureBank.Initialize(GraphicsDevice);
            FontBank.Initialize(Content);
            ModelBank.Initialize(Content);

            // Initialize the page manager and add the main page.
            Page = new MainPage(this);
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Page.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override async void Draw(GameTime gameTime)
        {
            // Clears the screen with the MonoGame orange color before drawing.
            GraphicsDevice.Clear(Color.AliceBlue);

            Page.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}