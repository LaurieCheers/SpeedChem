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
        public class WeaponSlot
        {
            public Weapon weapon;

            public WeaponSlot(Weapon weapon) { this.weapon = weapon; }
        }

        public WeaponSlot leftWeapon;
        public WeaponSlot rightWeapon;
        public Dictionary<string, Weapon> unlockableWeapons;
        public List<Weapon> availableWeapons;
        public bool newWeaponAdded = false;

        public int money { get; private set; }
        public int crystals { get; private set; }
        public bool cityJustUnlocked = false;

        float incomePerSecond;
        bool showIncomePerSecond = false;
        bool showCrystals = false;

        public bool pressLeftTutorial = true;
        public bool pressRightTutorial = true;
        public bool pressJumpTutorial = true;

        public Inventory()
        {
            unlockableWeapons = new Dictionary<string, Weapon>();
            AddUnlockable(new Weapon_Rivetgun());
            AddUnlockable(new Weapon_CuttingBeam());
            AddUnlockable(new Weapon_BubbleGun());
            AddUnlockable(new Weapon_Jetpack());

            Weapon rivetGun = unlockableWeapons["RIVETGUN"];

            leftWeapon = new WeaponSlot(rivetGun);
            rightWeapon = new WeaponSlot(null);
            availableWeapons = new List<Weapon> { rivetGun };
        }

        public void AddUnlockable(Weapon weapon)
        {
            unlockableWeapons.Add(weapon.ID, weapon);
        }

        public void UnlockWeapon(Weapon newWeapon)
        {
            availableWeapons.Add(newWeapon);
            rightWeapon.weapon = newWeapon;
            newWeaponAdded = true;
        }

        public bool PayMoney(int amount, Vector2 splashPos, SpeedChemScreen screen)
        {
            if (money >= amount)
            {
                money -= amount;

                if (amount > 0 && Game1.instance.currentScreen == screen)
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

        public bool SpendCrystals(int amount, Vector2 splashPos, SpeedChemScreen screen)
        {
            if (crystals >= amount)
            {
                crystals -= amount;

                if (amount > 0 && Game1.instance.currentScreen == screen)
                {
                    Game1.instance.splashes.Add(new Splash("bubble", TextAlignment.LEFT, Game1.font, Color.Red, splashPos, new Vector2(0, -2), 1.0f, 0.0f, 0.5f));
                }

                return true;
            }

            return false;
        }

        public void Update()
        {
            incomePerSecond = Game1.instance.worldLevel.incomePerSecond;
            if (incomePerSecond >= 1.0f)
                showIncomePerSecond = true;

            if (crystals > 0)
                showCrystals = true;
        }

        public void UpdateWeapons(InputState input, PlatformCharacter character, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            if (leftWeapon.weapon != null)
                leftWeapon.weapon.Update(input.hoveringElement == null ? input.mouseLeft : null, input.MousePos, character, allObjects, projectiles);
            if (rightWeapon.weapon != null)
                rightWeapon.weapon.Update(input.hoveringElement == null ? input.mouseRight : null, input.MousePos, character, allObjects, projectiles);
        }

        public void DrawWeapons(SpriteBatch spriteBatch)
        {
            if (leftWeapon.weapon != null)
                leftWeapon.weapon.Draw(spriteBatch);
            if (rightWeapon.weapon != null)
                rightWeapon.weapon.Draw(spriteBatch);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font, "$" + money, new Vector2(10, 10), Color.Yellow);

            if (showIncomePerSecond)
                spriteBatch.DrawString(Game1.font, "($" + (int)incomePerSecond + "/s)", new Vector2(10, 30), Color.Yellow);

            if(showCrystals)
                spriteBatch.DrawString(Game1.font, "" + crystals + " bubbles", new Vector2(10, 50), Color.Orange);

            if (cityJustUnlocked)
            {
                spriteBatch.DrawString(Game1.font, "New city available on the map screen!", new Vector2(300, 5), TextAlignment.CENTER, Color.Orange);
            }
        }
    }
}
