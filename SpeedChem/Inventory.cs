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

/*        const int HISTORY_LENGTH_SECS = 20;
        const int HISTORY_LENGTH_FRAMES = 60 * HISTORY_LENGTH_SECS;
        const int HISTORY_HALF_LENGTH_FRAMES = HISTORY_LENGTH_FRAMES / 2;
        int[] moneyHistory;
        int nextHistoryIdx = 0;
        long moneyTotal20_10 = 0;
        long moneyTotal10_00 = 0;*/
        float incomePerSecond;
        bool showIncomePerSecond = false;
        bool showCrystals = false;

        public Inventory()
        {
/*            moneyHistory = new int[HISTORY_LENGTH_FRAMES];
            for(int Idx = 0; Idx < moneyHistory.Length; ++Idx)
            {
                moneyHistory[Idx] = 0;
            }*/
        }

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

        public void Update()
        {
            /*            moneyTotal20_10 -= moneyHistory[nextHistoryIdx];
                        moneyHistory[nextHistoryIdx] = money;
                        moneyTotal10_00 += money;
                        nextHistoryIdx = (nextHistoryIdx+1)% HISTORY_LENGTH_FRAMES;

                        int halfwayValue = moneyHistory[(nextHistoryIdx + HISTORY_HALF_LENGTH_FRAMES) % HISTORY_LENGTH_FRAMES];
                        moneyTotal10_00 -= halfwayValue;
                        moneyTotal20_10 += halfwayValue;

                        incomePerSecond = (moneyTotal10_00 - moneyTotal20_10) / (10.0f*HISTORY_HALF_LENGTH_FRAMES);
            */
            incomePerSecond = Game1.instance.worldLevel.incomePerSecond;
            if (incomePerSecond >= 1.0f)
                showIncomePerSecond = true;

            if (crystals > 0)
                showCrystals = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font, "$" + money, new Vector2(10, 10), Color.Yellow);

            if (showIncomePerSecond)
                spriteBatch.DrawString(Game1.font, "($" + (int)incomePerSecond + "/s)", new Vector2(10, 30), Color.Yellow);

            if(showCrystals)
                spriteBatch.DrawString(Game1.font, "" + crystals + " crystals", new Vector2(10, 50), Color.Orange);

            if (cityJustUnlocked)
            {
                spriteBatch.DrawString(Game1.font, "New city available on the map screen!", new Vector2(300, 5), TextAlignment.CENTER, Color.Orange);
            }
        }
    }
}
