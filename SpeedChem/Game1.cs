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
    public class Game1 : Game
    {
        public static Game1 instance;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputState inputState = new InputState();
        List<WorldObject> objects;
        List<Projectile> projectiles;
        List<Triggerable> triggerables;

        public Texture2D blockTexture;
        public Texture2D whiteTexture;

        public Game1()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            objects = new List<WorldObject>();
            projectiles = new List<Projectile>();
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blockTexture = Content.Load<Texture2D>("button3d");
            Texture2D characterSprite = Content.Load<Texture2D>("bodyguard");
            whiteTexture = Content.Load<Texture2D>("white");
            Texture2D clearTexture = Content.Load<Texture2D>("clear");

            objects = new List<WorldObject>();
            objects.Add(new WorldObject(blockTexture, new Vector2(0, 0), new Vector2(32, 192)));
            objects.Add(new WorldObject(blockTexture, new Vector2(192, 0), new Vector2(160, 32)));
            objects.Add(new WorldObject(blockTexture, new Vector2(352, 0), new Vector2(32, 320)));
            objects.Add(new WorldObject(blockTexture, new Vector2(16, 124), new Vector2(32, 32)));
            objects.Add(new WorldObject(blockTexture, new Vector2(0, 192), new Vector2(224, 32)));
            objects.Add(new WorldObject(blockTexture, new Vector2(192, 224), new Vector2(32, 96)));

            Spawner spawner = new Spawner(new Vectangle(92, 32, 78, 36), new Color[,] { { Color.White, Color.Green } });
            triggerables = new List<Triggerable>();
            triggerables.Add(spawner);

            objects.Add(new PushButton(spawner, clearTexture, whiteTexture, new Vector2(32, 160), new Vector2(8, 32), Color.Red));

/*            ChemBlock a = new ChemBlock(blockTexture, new Vector2(80, 60), new Vector2(32, 32), Color.Red);
            ChemBlock b = new ChemBlock(blockTexture, new Vector2(112, 60), new Vector2(32, 32), Color.Green);
            ChemBlock c = new ChemBlock(blockTexture, new Vector2(112, 92), new Vector2(32, 32), Color.Purple);
//            a.BondWith(b);
//            a.BondWith(c);
            objects.Add(a);
            objects.Add(b);
            objects.Add(c);*/

//            ChemBlock d = new ChemBlock(blockTexture, new Vector2(95, 130), new Vector2(32, 32), Color.Yellow);
//            ChemBlock e = new ChemBlock(blockTexture, new Vector2(95, 162), new Vector2(32, 32), Color.LightBlue);
//            d.BondWith(e);
//            objects.Add(d);
//            objects.Add(e);

            PlatformCharacter player = new PlatformCharacter(characterSprite, new Vector2(50, 100), new Vector2(22, 32), Color.White, new Rectangle(10, 0, 22, 32));
            objects.Add(player);

            //projectiles.Add(new Projectile(whiteTexture, new Vector2(300, 100), new Vector2(15, 3.0f), new Vector2(1,0)));
            // TODO: use this.Content to load your game content here
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

            foreach(WorldObject obj in objects)
            {
                obj.Update(inputState, objects, projectiles);
            }
            for (int pIdx = projectiles.Count-1; pIdx >= 0; --pIdx)
            {
                projectiles[pIdx].Update(objects);
                if (projectiles[pIdx].destroyed)
                {
                    projectiles[pIdx] = projectiles[projectiles.Count - 1];
                    projectiles.RemoveAt(projectiles.Count - 1);
                }
            }
            foreach(Triggerable triggerable in triggerables)
            {
                triggerable.Update(objects);
            }

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
            foreach(WorldObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
