using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SpeedChem
{
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

        public MetaGame metaGame;
        public GameLevel level;

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

            spriteBatch = new SpriteBatch(GraphicsDevice);
            textures = new TextureCache(Content);
            buttonStyle = UIButton.GetDefaultStyle(Content);
            metaGame = new MetaGame();
            level = new GameLevel();
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

            level.Update(inputState);
            metaGame.Update(inputState, level.active);
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

            spriteBatch.Draw(textures.grass, new Rectangle(0, 0, 800, 600), Color.White);

            if (level.active)
                level.Draw(spriteBatch);
            else
                metaGame.Draw(spriteBatch);

            splashes.Draw(spriteBatch);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        public void ViewFactory(ChemicalFactory factory)
        {
            level.Open(factory);
        }
    }
}
