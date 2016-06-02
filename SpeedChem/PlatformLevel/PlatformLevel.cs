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
    public class PlatformLevel: SpeedChemScreen
    {
        List<PlatformObject> objects;
        List<Projectile> projectiles;
        List<Command> triggerables;
        UIContainer ui;
        ChemicalFactory factory;
        public UIButton saveButton;
        List<FactoryCommand> recordedCommands = new List<FactoryCommand>();
        PlatformCharacter player;
        Weapon rivetGun = new Weapon_Rivetgun();
        Weapon cuttingBeam = new Weapon_CuttingBeam();
        Weapon bubblegun = new Weapon_BubbleGun();
        UIRadioButtonGroup<Weapon> weaponButtons = new UIRadioButtonGroup<Weapon>();
        UIRadioButton<Weapon> rivetGunButton;
        UIRadioButton<Weapon> cuttingBeamButton;
        UIRadioButton<Weapon> bubblegunButton;
        int currentTime;
        bool paused;
        bool timerRunning = false;

        public PlatformLevel()
        {
            //InitObjects();

            ui = new UIContainer();
            ui.Add(new UIButton("Reset", new Rectangle(500,30, 100,50), Game1.buttonStyle, button_Reset));
            ui.Add(new UIButton("Cancel", new Rectangle(500, 90, 100, 50), Game1.buttonStyle, button_Cancel));

            saveButton = new UIButton("Save", new Rectangle(500, 150, 100, 50), Game1.buttonStyle, button_Save);
            ui.Add(saveButton);

            UIButtonStyle weaponButtonStyle = new UIButtonStyle(
                new UIButtonAppearance(Game1.font, Color.White, Game1.textures.castIronButton, Color.White, new Vector2(20,0)),
                new UIButtonAppearance(Game1.font, Color.White, Game1.textures.castIronButton_hover, Color.White, new Vector2(20, 0)),
                new UIButtonAppearance(Game1.font, Color.White, Game1.textures.castIronButton_pressed, Color.White, new Vector2(20, 0)),
                new UIButtonAppearance(Game1.font, Color.White, Game1.textures.castIronButton, Color.White, new Vector2(20, 0))
            );

            UIButtonAppearance weaponSelectedStyle = new UIButtonAppearance(Game1.font, Color.Black, Game1.textures.castIronButton_active, Color.White, new Vector2(20, 0));

            rivetGunButton = new UIRadioButton<Weapon>("Rivet Gun", rivetGun, weaponButtons, new Rectangle(500, 220, 170, 50), weaponButtonStyle, weaponSelectedStyle, button_SelectWeapon);
            ui.Add(rivetGunButton);
            cuttingBeamButton = new UIRadioButton<Weapon>("Cutting Beam", cuttingBeam, weaponButtons, new Rectangle(500, 270, 170, 50), weaponButtonStyle, weaponSelectedStyle, button_SelectWeapon);
            bubblegunButton = new UIRadioButton<Weapon>("Bubblegun", bubblegun, weaponButtons, new Rectangle(500, 320, 170, 50), weaponButtonStyle, weaponSelectedStyle, button_SelectWeapon);
            ui.Add(bubblegunButton);
        }

        public void UnlockCuttingBeam()
        {
            ui.Add(cuttingBeamButton);
        }

        void InitObjects()
        {
            objects = new List<PlatformObject>();
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(-16, 0), new Vector2(32, 136)));
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(0, 136), new Vector2(32, 136)));
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(384, 0), new Vector2(32, 128)));
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(384, 128), new Vector2(32, 128)));
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(384, 256), new Vector2(32, 128)));
            objects.Add(new PlatformObject(Game1.textures.buttonHood, new Vector2(16, 204), new Vector2(32, 32)));
            objects.Add(new PlatformObject(Game1.textures.woodFloor, new Vector2(0, 272), new Vector2(192, 32)));
            objects.Add(new PlatformObject(Game1.textures.cement, new Vector2(192, 272), new Vector2(32, 128)));

            if (factory.internalSeller != null)
            {
                objects.Add(new SellerZone(factory.internalSeller, factory.sellerPrice, factory.sellerAction, new Vector2(192, 300), new Vector2(160, 32)));
            }
            else
            {
                //ChemicalSignature inputChemical = new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.GREEN });
                ChemicalSignature outputChemical = null;
                PipeSocket receiver = factory.pipes.First().connectedTo;
                if (receiver != null)
                {
                    outputChemical = receiver.parent.GetInputChemical();
                }

                objects.Add(new OutputZone(outputChemical, new Vector2(192, 300), new Vector2(160, 32)));
            }

            triggerables = new List<Command>();
            Command_Spawn spawner = new Command_Spawn(new Vectangle(92, -36, 78, 32), factory, 0);
            triggerables.Add(spawner);

            objects.Add(new PushButton(spawner, Game1.textures.clear, Game1.textures.white, new Vector2(32, 240), new Vector2(8, 32), Color.Red));

            if (factory.pipeSocket.connectedPipes.Count > 1)
            {
                objects.Add(new PlatformObject(Game1.textures.buttonHood, new Vector2(366, 28), new Vector2(32, 32)));
                objects.Add(new PlatformObject(Game1.textures.woodFloor, new Vector2(192, 96), new Vector2(192, 32)));

                Command_Spawn spawner2 = new Command_Spawn(new Vectangle(256, -36, 78, 32), factory, 1);
                triggerables.Add(spawner2);
                objects.Add(new PushButton(spawner2, Game1.textures.clear, Game1.textures.white, new Vector2(384 - 8, 64), new Vector2(8, 32), Color.Red));
            }

            player = new PlatformCharacter(Game1.textures.character, new Vector2(50, 200), new Vector2(14, 32), Color.White, new Rectangle(9, 0, 14, 32));
            objects.Add(player);

            weaponButtons.selectedButton = rivetGunButton;

            projectiles = new List<Projectile>();
            paused = false;
            recordedCommands.Clear();
            currentTime = 0;
            UpdateAnyBlocksLeft();
        }

        public void Record(FactoryCommandType commandType)
        {
            recordedCommands.Add(new FactoryCommand(currentTime, commandType));
        }

        public void Record(FactoryCommandType commandType, int sellPrice)
        {
            recordedCommands.Add(new FactoryCommand(currentTime, commandType, sellPrice));
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
            Game1.instance.ViewCity(factory.cityLevel);
        }

        public void button_Save()
        {
            factory.SaveRecording(recordedCommands);
            Game1.instance.ViewCity(factory.cityLevel);
        }

        public void button_SelectWeapon(Weapon weapon)
        {
            player.SelectWeapon(weapon);
        }

        public void Update(InputState inputState)
        {
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
                if(timerRunning)
                    currentTime++;

                bool needsDestroy = false;
                foreach (PlatformObject obj in objects)
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

            inputState.hoveringElement = ui.GetMouseHover(inputState.MousePos);
            ui.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.textures.levelbg, new Rectangle(0, 0, 800, 600), Color.White);

            foreach (PlatformObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }

            ui.Draw(spriteBatch);

            spriteBatch.Draw(Game1.textures.rivetgun, new Vector2(508, 228), Color.White);
            spriteBatch.Draw(Game1.textures.cutting_laser, new Vector2(508, 278), Color.White);

            spriteBatch.DrawString(Game1.font, TimeToString(currentTime), new Vector2(500, 10), timerRunning ? Color.Yellow: Color.White);
            spriteBatch.Draw(timerRunning ? Game1.textures.hourglass : Game1.textures.hourglass_frozen, new Vector2(475, 0), Color.White);

            int currentMultiplier = NumThreadsForTime(currentTime/60.0f);

            int testTime = ChemicalFactory.TIME_PER_CORE;
            int testMultiplier = NumThreadsForTime(testTime);
            int lastMultiplier = 0;

            Vector2 currentPos = new Vector2(508, 400);
            while (true)
            {
                if (testMultiplier != lastMultiplier)
                {
                    spriteBatch.DrawString(Game1.font, (testMultiplier == 1) ? "Normal" : ("Under " + testTime + " seconds: "+testMultiplier + "x speed"), currentPos, (currentMultiplier == testMultiplier)? Color.White: Color.Gray);
                    currentPos.Y += 15;
                    lastMultiplier = testMultiplier;
                }
                if (testMultiplier == 1)
                {
                    break;
                }

                testTime += ChemicalFactory.TIME_PER_CORE;
                testMultiplier = NumThreadsForTime(testTime);
            }
        }

        public int NumThreadsForTime(float duration)
        {
            float currentCoresPerThread = (float)Math.Ceiling(duration / (float)ChemicalFactory.TIME_PER_CORE);
            int currentNumThreads = (int)(factory.numCores / currentCoresPerThread);
            if (currentNumThreads == 0)
                currentNumThreads = 1;

            return currentNumThreads;
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

        public void UpdateAnyBlocksLeft()
        {
            timerRunning = false;
            foreach (PlatformObject obj in objects)
            {
                if (obj is ChemBlock && !obj.destroyed)
                {
                    timerRunning = true;
                    break;
                }
            }
            Game1.instance.platformLevel.saveButton.SetEnabled(!timerRunning && recordedCommands.Count > 0);
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
