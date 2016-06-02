using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SpeedChem
{
    public interface SpeedChemScreen
    {
        void Update(InputState inputState);
        void Draw(SpriteBatch spriteBatch);
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1: Game
    {
        public const float BLOCKSIZE = 32.0f;
        public static Game1 instance;
        public static TextureCache textures;
        public static UIButtonStyle buttonStyle;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputState inputState = new InputState();
        public static SpriteFont font;
        public SplashManager splashes;
        public Inventory inventory = new Inventory();

        public SpeedChemScreen currentScreen;
        public WorldLevel worldLevel;
        public PlatformLevel platformLevel;

        public Game1()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            font = Content.Load<SpriteFont>("Arial");

            JSONTable settings = new JSONTable("Settings.json");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            textures = new TextureCache(Content);
            buttonStyle = UIButton.GetDefaultStyle(Content);
            worldLevel = new WorldLevel(settings.getJSON("cities"));
            platformLevel = new PlatformLevel();
            currentScreen = worldLevel;
            splashes = new SplashManager();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            inputState.Update();

            inventory.Update();
            worldLevel.Run();
            currentScreen.Update(inputState);

            splashes.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
                currentScreen.Draw(spriteBatch);
                inventory.Draw(spriteBatch);
                splashes.Draw(spriteBatch);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        public void ViewFactory(ChemicalFactory factory)
        {
            platformLevel.Open(factory);
            this.currentScreen = platformLevel;
        }

        public void ViewCity(CityLevel cityLevel)
        {
            this.currentScreen = cityLevel;
        }

        public void ViewWorld()
        {
            this.currentScreen = worldLevel;
        }

        public static float FramesToSeconds(int frames)
        {
            return frames / 60.0f;
        }
    }
}
