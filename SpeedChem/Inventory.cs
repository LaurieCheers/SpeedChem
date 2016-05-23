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
    public class Inventory
    {
        public int money { get; private set; }
        public int crystals { get; private set; }
        public bool cityJustUnlocked = false;

        public bool PayMoney(int amount, Vector2 splashPos, SpeedChemScreen screen)
        {
            if (money >= amount)
            {
                money -= amount;

                if (Game1.instance.currentScreen == screen)
                {
                    Game1.instance.splashes.Add(new Splash("-$" + amount, TextAlignment.LEFT, Game1.font, Color.Red, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
                }

                return true;
            }

            return false;
        }

        public void GainMoney(int amount, Vector2 splashPos, SpeedChemScreen screen)
        {
            money += amount;

            if (Game1.instance.currentScreen == screen)
            {
                Game1.instance.splashes.Add(new Splash("+$" + amount, TextAlignment.LEFT, Game1.font, Color.Yellow, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
            }
        }

        public void GainCrystals(int n)
        {
            crystals += n;
        }

        public bool SpendCrystals(int n)
        {
            if (crystals >= n)
            {
                crystals -= n;
                return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font, "$" + money, new Vector2(10, 10), Color.Yellow);
            spriteBatch.DrawString(Game1.font, "" + crystals + " crystals", new Vector2(10, 30), Color.Orange);
            if (cityJustUnlocked)
            {
                spriteBatch.DrawString(Game1.font, "New city available on the map screen!", new Vector2(300, 5), TextAlignment.CENTER, Color.Orange);
            }
        }
    }
}
