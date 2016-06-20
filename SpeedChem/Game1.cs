using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
    public class Game1 : Game
    {
        public const float BLOCKSIZE = 32.0f;
        public static Game1 instance;
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

        List<CustomDrawInstruction> drawInstructions = new List<CustomDrawInstruction>();

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
            TextureCache.Load(Content);
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

        float testAngle = 0;
        int testFrames = 0;

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

            foreach(CustomDrawInstruction instruction in drawInstructions)
            {
                instruction.Draw();
            }
            drawInstructions.Clear();

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

        public void AddDrawInstruction(CustomDrawInstruction instruction)
        {
            drawInstructions.Add(instruction);
        }
    }

    public abstract class CustomDrawInstruction
    {
        public abstract void Draw();

        public static BasicEffect CreateScreenspaceEffect()
        {
            Matrix viewMatrix = Matrix.CreateLookAt(
                new Vector3(0.0f, 0.0f, 1.0f),
                Vector3.Zero,
                Vector3.Up
            );

            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0,
                (float)Game1.instance.GraphicsDevice.Viewport.Width,
                (float)Game1.instance.GraphicsDevice.Viewport.Height,
                0,
                1.0f, 1000.0f
            );

            BasicEffect basicEffect = new BasicEffect(Game1.instance.GraphicsDevice);
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            return basicEffect;
        }
    }

    class CustomDrawInstruction_Clock: CustomDrawInstruction
    {
        BasicEffect basicEffect;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        const int numTris = 9;

        public CustomDrawInstruction_Clock(Texture2D texture, Vector2 center, float radius, Color color, float startAngle, float endAngle)
        {
            basicEffect = CreateScreenspaceEffect();
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            VertexPositionTexture[] primitiveList = new VertexPositionTexture[numTris + 2];
            float step = (endAngle - startAngle) / numTris;
            short[] indices = new short[numTris * 3];

            primitiveList[0] = new VertexPositionTexture(new Vector3(center.X, center.Y, 0), new Vector2(0.5f, 0.5f));
            for (int x = 0; x <= numTris; x++)
            {
                double angle = startAngle + x * step;
                double xBase = Math.Sin(angle);
                double yBase = -Math.Cos(angle);
                primitiveList[x + 1] = new VertexPositionTexture(new Vector3((float)(center.X + xBase * radius), (float)(center.Y + yBase * radius), 0), new Vector2(((float)xBase + 1) * 0.5f, ((float)yBase + 1) * 0.5f));
                if (x < numTris)
                {
                    indices[x * 3] = 0;
                    indices[x * 3 + 2] = (short)(x + 2);
                    indices[x * 3 + 1] = (short)(x + 1);
                }
            }

            vertexBuffer = new VertexBuffer(Game1.instance.GraphicsDevice, VertexPositionTexture.VertexDeclaration, primitiveList.Length, BufferUsage.None);
            vertexBuffer.SetData(primitiveList);

            indexBuffer = new IndexBuffer(Game1.instance.GraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);
        }

        public override void Draw()
        {
            Game1.instance.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Game1.instance.GraphicsDevice.Indices = indexBuffer;

            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            Game1.instance.GraphicsDevice.RasterizerState = rasterizerState1;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Game1.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numTris);
            }
        }
    }
}
