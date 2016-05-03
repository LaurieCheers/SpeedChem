using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    public class GameLevel
    {
        List<WorldObject> objects;
        List<Projectile> projectiles;
        List<Command> triggerables;
        UIContainer ui;
        public bool active { get; private set; }
        ChemicalFactory factory;
        public UIButton saveButton;
        List<FactoryCommand> recordedCommands = new List<FactoryCommand>();
        int currentTime;
        bool paused;

        public GameLevel()
        {
            //InitObjects();

            ui = new UIContainer();
            ui.Add(new UIButton("Reset", new Rectangle(500,30, 100,50), Game1.buttonStyle, button_Reset));
            ui.Add(new UIButton("Cancel", new Rectangle(500, 90, 100, 50), Game1.buttonStyle, button_Cancel));
            saveButton = new UIButton("Save", new Rectangle(500, 150, 100, 50), Game1.buttonStyle, button_Save);
            ui.Add(saveButton);
        }

        void InitObjects()
        {
            objects = new List<WorldObject>();
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(0, 0), new Vector2(16, 128)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(0, 128), new Vector2(32, 128)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(384, 0), new Vector2(32, 320)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(16, 188), new Vector2(32, 32)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(0, 256), new Vector2(224, 32)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(192, 288), new Vector2(32, 96)));

            //ChemicalSignature inputChemical = new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.GREEN });
            ChemicalSignature outputChemical = null;
            PipeSocket receiver = factory.pipes.First().connectedTo;
            if(receiver != null)
            {
                outputChemical = receiver.parent.GetInputChemical();
            }

            triggerables = new List<Command>();
            Command_Spawn spawner = new Command_Spawn(new Vectangle(92, 32, 78, -36), factory, 0);
            triggerables.Add(spawner);

            objects.Add(new PushButton(spawner, Game1.textures.clear, Game1.textures.white, new Vector2(32, 224), new Vector2(8, 32), Color.Red));
            objects.Add(new OutputZone(outputChemical, Game1.textures.white, new Vector2(192, 300), new Vector2(160, 32)));

            if (factory.pipeSocket.connectedPipes.Count > 1)
            {
                objects.Add(new WorldObject(Game1.textures.block, new Vector2(368, 32), new Vector2(32, 32)));
                objects.Add(new WorldObject(Game1.textures.block, new Vector2(192, 96), new Vector2(192, 32)));

                Command_Spawn spawner2 = new Command_Spawn(new Vectangle(256, 32, 78, -36), factory, 1);
                triggerables.Add(spawner2);
                objects.Add(new PushButton(spawner2, Game1.textures.clear, Game1.textures.white, new Vector2(384 - 8, 64), new Vector2(8, 32), Color.Red));
            }

            PlatformCharacter player = new PlatformCharacter(Game1.textures.character, new Vector2(50, 200), new Vector2(22, 32), Color.White, new Rectangle(10, 0, 22, 32));
            objects.Add(player);

            projectiles = new List<Projectile>();
            active = true;
            paused = true;
            recordedCommands.Clear();
            currentTime = 0;
        }

        public void Open(ChemicalFactory factory)
        {
            this.factory = factory;
            InitObjects();
        }

        public void button_Reset()
        {
            InitObjects();
        }

        public void button_Cancel()
        {
            active = false;
        }

        public void button_Save()
        {
            factory.SaveRecording(recordedCommands);
            active = false;
        }

        public void Update(InputState inputState)
        {
            if (!active)
                return;

            if (paused)
            {
                if (inputState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)
                    || inputState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)
                    || inputState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)
                    || inputState.WasMouseRightJustPressed()
                    || (inputState.WasMouseLeftJustPressed() && inputState.MousePos.X < 500 ))
                {
                    paused = false;
                }
            }

            if (!paused)
            {
                currentTime++;

                bool needsDestroy = false;
                foreach (WorldObject obj in objects)
                {
                    obj.Update(inputState, objects, projectiles);
                    if (obj.destroyed)
                        needsDestroy = true;
                }
                if (needsDestroy)
                {
                    for (int oIdx = objects.Count - 1; oIdx >= 0; --oIdx)
                    {
                        if (objects[oIdx].destroyed)
                        {
                            objects[oIdx] = objects[objects.Count - 1];
                            objects.RemoveAt(objects.Count - 1);
                        }
                    }
                }
                for (int pIdx = projectiles.Count - 1; pIdx >= 0; --pIdx)
                {
                    projectiles[pIdx].Update(objects);
                    if (projectiles[pIdx].destroyed)
                    {
                        projectiles[pIdx] = projectiles[projectiles.Count - 1];
                        projectiles.RemoveAt(projectiles.Count - 1);
                    }
                }
                foreach (Command triggerable in triggerables)
                {
                    triggerable.Update(objects);
                }
            }

            ui.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!active)
                return;

            foreach (WorldObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }

            ui.Draw(spriteBatch);

            spriteBatch.DrawString(Game1.font, TimeToString(currentTime), new Vector2(500, 10), paused? Color.Pink: Color.White);
        }

        public void ProduceChemical(ChemicalSignature signature)
        {
            recordedCommands.Add(new FactoryCommand(currentTime, FactoryCommandType.OUTPUT, signature));

//            factory.PushOutput(signature);
        }

        public ChemicalSignature SpawnInputChemical(int inputIndex)
        {
            ChemicalSignature signature = factory.GetInputChemical(inputIndex);
            if(signature != null)
            {
                recordedCommands.Add(new FactoryCommand(currentTime, FactoryCommandType.INPUT, signature));
            }
            return signature;
        }

        public void UpdateSaveButton()
        {
            bool anyBlocksLeft = false;
            foreach (WorldObject obj in objects)
            {
                if (obj is ChemBlock && !obj.destroyed)
                {
                    anyBlocksLeft = true;
                    break;
                }
            }
            Game1.instance.level.saveButton.SetEnabled(!anyBlocksLeft);
        }

        public static string TimeToString(int time)
        {
            string millis = "" + (time % 60);
            if (millis.Length == 1)
                millis = "0" + millis;

            return "" + (time / 60) + ":" + millis;
        }
    }
}
