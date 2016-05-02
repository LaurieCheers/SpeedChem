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
        UIContainer ui;
        //Vector2 nextSiloPos = new Vector2(100,300);
        //Dictionary<ChemicalSignature, ChemicalSilo> chemicals = new Dictionary<ChemicalSignature, ChemicalSilo>();
        object selectedObject;
        bool isBackground;

        public MetaGame()
        {
            int nextInputX = 200;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.WHITE }), 0, new Vector2(nextInputX, 30)));
            nextInputX += 100;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.BLUE }), 5, new Vector2(nextInputX, 30)));
            nextInputX += 100;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.RED }), 80, new Vector2(nextInputX, 30)));

            int nextOutputX = 200;
            objects.Add(new ChemicalOutbox
            (
                new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.WHITE }),
                1,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += 100;
            objects.Add(new ChemicalOutbox
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                                ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE
                }),
                30,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += 100;
            objects.Add(new ChemicalOutbox
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE
                }),
                500,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += 100;
            objects.Add(new ChemicalOutbox
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.RED,
                    ChemicalElement.RED, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE
                }),
                1000,
                new Vector2(nextOutputX, 350)
            ));


            ui = new UIContainer();
            ui.Add(new UIButton("New Factory", new Rectangle(10, 30, 120, 40), Game1.buttonStyle, button_SpawnFactory));
            ui.Add(new UIButton("New Silo", new Rectangle(10, 75, 120, 40), Game1.buttonStyle, button_SpawnSilo));
        }

        public void button_SpawnFactory()
        {
            objects.Add(new ChemicalFactory(new Vector2(50, 200)));
        }

        public void button_SpawnSilo()
        {
            objects.Add(new ChemicalSilo(new Vector2(50, 200)));
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
        }

        public bool PayMoney(int amount, Vector2 splashPos)
        {
            if (money >= amount)
            {
                money -= amount;

                if(!isBackground)
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

            if (!isBackground)
            {
                Game1.instance.splashes.Add(new Splash("+$" + amount, Game1.font, Color.Yellow, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
            }
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
