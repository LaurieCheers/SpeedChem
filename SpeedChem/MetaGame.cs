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
    public class MetaGame
    {
        List<MetaGameObject> objects = new List<MetaGameObject>();
        int money = 0;
        int laserCrystals;
        UIContainer ui;
        //Vector2 nextSiloPos = new Vector2(100,300);
        //Dictionary<ChemicalSignature, ChemicalSilo> chemicals = new Dictionary<ChemicalSignature, ChemicalSilo>();
        object selectedObject;
        bool isBackground;
        int nextFactoryPrice = 80;
        UIButton newFactoryButton;

        public MetaGame()
        {
            int nextInputX = 100;
            int inputSpacingX = 100;
            ChemicalInbox tutorialInbox = new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.WHITE }), 0, new Vector2(nextInputX, 30));
            objects.Add(tutorialInbox);
            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.BLUE }), 5, new Vector2(nextInputX, 30)));
            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox(new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.RED }), 80, new Vector2(nextInputX, 30)));
            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.GLASS }), 10000, new Vector2(nextInputX, 30)));

            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox
            (
                new ChemicalSignature(1, new ChemicalElement[] {
                    ChemicalElement.BLUE,
                    ChemicalElement.BLUE,
                }),
                0,
                new Vector2(nextInputX, 30)
            ));

            int nextOutputX = 20;
            const int outputSpacingX = 87;
            /*ChemicalOutbox tutorialOutbox = new ChemicalOutbox
            (
                new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.WHITE }),
                1,
                new Vector2(nextOutputX, 350)
            );*/
            ChemicalFactory tutorialOutbox = new ChemicalFactory
            (
                new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.WHITE }),
                1,
                new Vector2(nextOutputX, 150)
            );
            objects.Add(tutorialOutbox);

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(1, new ChemicalElement[] {
                    ChemicalElement.BLUE,
                    ChemicalElement.BLUE,
                }),
                15,
                new Vector2(nextOutputX, 150)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.NONE,ChemicalElement.WHITE,ChemicalElement.NONE,
                    ChemicalElement.WHITE,ChemicalElement.RED,ChemicalElement.WHITE,
                    ChemicalElement.NONE,ChemicalElement.WHITE,ChemicalElement.NONE,
                }),
                FactoryCommandType.GAINCRYSTAL,
                new Vector2(nextOutputX, 180)
            ));


            nextOutputX = 60;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                                ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE
                }),
                35,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE
                }),
                300,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.RED,
                    ChemicalElement.RED, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE
                }),
                1000,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(2, new ChemicalElement[] {
                    ChemicalElement.GLASS, ChemicalElement.BLUE,
                    ChemicalElement.BLUE, ChemicalElement.GLASS,
                }),
                28000,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.GLASS, ChemicalElement.NONE, ChemicalElement.GLASS,
                    ChemicalElement.BLUE, ChemicalElement.GLASS, ChemicalElement.BLUE,
                }),
                46000,
                new Vector2(nextOutputX, 350)
            ));

            //            ChemicalFactory tutorialFactory = new ChemicalFactory(new Vector2(100, 200));
            //            objects.Add(tutorialFactory);

            //            tutorialInbox.pipes.First().ConnectTo(tutorialFactory.pipeSocket);
            //            tutorialFactory.pipes.First().ConnectTo(tutorialOutbox.pipeSocket);
            tutorialInbox.pipes.First().ConnectTo(tutorialOutbox.pipeSocket);

            //objects.Add(new ChemicalFactory(new Vector2(400, 200)));

            //            selectedObject = tutorialFactory;
            selectedObject = tutorialOutbox;

            ui = new UIContainer();
            newFactoryButton = new UIButton(GetFactoryButtonLabel(), new Rectangle(600, 100, 170, 40), Game1.buttonStyle, button_SpawnFactory);
            ui.Add(newFactoryButton);
            ui.Add(new UIButton("New Silo", new Rectangle(600, 150, 120, 40), Game1.buttonStyle, button_SpawnSilo));
            ui.Add(new UIButton("Cheat:Loadsamoney", new Rectangle(600, 420, 170, 40), Game1.buttonStyle, button_CheatMoney));

            MoneyChanged();
        }

        public void button_SpawnFactory()
        {
            Vector2 factoryPos = new Vector2(50, 200);
            if (PayMoney(nextFactoryPrice, factoryPos))
            {
                objects.Add(new ChemicalFactory(factoryPos));
                nextFactoryPrice = (int)(nextFactoryPrice * 5f);
                newFactoryButton.label = GetFactoryButtonLabel();
                MoneyChanged();
            }
        }

        string GetFactoryButtonLabel()
        {
            return "Build Factory ($" + nextFactoryPrice + ")";
        }

        public void button_SpawnSilo()
        {
            objects.Add(new ChemicalSilo(new Vector2(50, 200)));
        }

        public void button_CheatMoney()
        {
            GainMoney(1000000, Vector2.Zero);
            laserCrystals += 100;
        }

        public void Update(InputState inputState, bool isBackground)
        {
            this.isBackground = isBackground;
            foreach (MetaGameObject obj in objects)
            {
                if(obj is ChemicalFactory)
                    (obj as ChemicalFactory).Run();
            }

            if (!isBackground)
            {
                foreach (MetaGameObject obj in objects)
                {
                    obj.Update(inputState, ref selectedObject);

                    foreach(OutputPipe pipe in obj.pipes)
                    {
                        pipe.Update(inputState, this, ref selectedObject);
                    }
                }

                ui.Update(inputState);

                if (inputState.WasMouseLeftJustReleased() && selectedObject is MetaGameObject && !((MetaGameObject)selectedObject).bounds.Contains(inputState.MousePos))
                    selectedObject = null;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(MetaGameObject obj in objects)
            {
                foreach(OutputPipe pipe in obj.pipes)
                {
                    pipe.Draw(spriteBatch);
                }
            }

            foreach (MetaGameObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }

            ui.Draw(spriteBatch);

            spriteBatch.DrawString(Game1.font, "$" + money, new Vector2(10, 10), Color.Yellow);
            spriteBatch.DrawString(Game1.font, "" + laserCrystals+" crystals", new Vector2(10, 30), Color.Orange);
        }

        public bool PayMoney(int amount, Vector2 splashPos)
        {
            if (money >= amount)
            {
                money -= amount;
                MoneyChanged();

                if (!isBackground)
                {
                    Game1.instance.splashes.Add(new Splash("-$"+amount, Game1.font, Color.Red, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
                }

                return true;
            }

            return false;
        }

        public void GainMoney(int amount, Vector2 splashPos)
        {
            money += amount;
            MoneyChanged();

            if (!isBackground)
            {
                Game1.instance.splashes.Add(new Splash("+$" + amount, Game1.font, Color.Yellow, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
            }
        }

        public void MoneyChanged()
        {
            newFactoryButton.SetEnabled(money >= nextFactoryPrice);
        }

        public void GainCrystal()
        {
            laserCrystals++;
        }

        public bool SpendCrystal()
        {
            if (laserCrystals > 0)
            {
                laserCrystals--;
                return true;
            }

            return false;
        }

        /*
        public void ProduceChemical(ChemicalSignature signature)
        {
            if(!chemicals.ContainsKey(signature))
            {
                ChemicalSilo newSilo = new ChemicalSilo(signature, amount, Game1.textures.silo, nextSiloPos, new Vector2(32, 32));
                objects.Add(newSilo);
                chemicals[signature] = newSilo;
                nextSiloPos.X += 50;
            }
            else
            {
                chemicals[signature].amount++;
            }
        }
        */

        public MetaGameObject GetObjectAt(Vector2 pos)
        {
            foreach(MetaGameObject obj in objects)
            {
                if (obj.bounds.Contains(pos))
                    return obj;
            }

            return null;
        }
    }
}
