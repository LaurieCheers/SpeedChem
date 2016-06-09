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
    class WeaponPlinth : CityObject
    {
        Weapon weapon;
        int price;
        int crystals;
        UIContainer ui = new UIContainer();
        bool purchased;

        public WeaponPlinth(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.plinth, template.getVector2("pos"), TextureCache.plinth.Size())
        {
            this.weapon = Game1.instance.inventory.unlockableWeapons[template.getString("weapon")];
            this.price = template.getInt("price", 0);
            this.canDrag = false;
            this.crystals = template.getInt("crystals", 0);
            Init();
        }

        public WeaponPlinth(CityLevel cityLevel, Weapon weapon, int price, Vector2 pos) : base(cityLevel, TextureCache.inbox, pos, TextureCache.inbox.Size())
        {
            this.weapon = weapon;
            this.price = price;
            this.canDrag = false;
            Init();
        }

        void Init()
        {
            ui.Add(new UIBuyButton("Buy", price, new Rectangle(-40, 56, 112, 35), Game1.buttonStyle, button_Purchase));
        }

        public void button_Purchase()
        {
            purchased = true;
            didOutput = true;
        }

        public override UIMouseResponder GetOverlayMouseHover(Vector2 localMousePos)
        {
            return ui.GetMouseHover(localMousePos);
        }

        public override void Update(CityUIBlackboard blackboard)
        {
            if (purchased)
            {
                Game1.instance.inventory.UnlockWeapon(weapon);
                blackboard.cityLevel.RemoveObjectDeferred(this);
                Game1.instance.inventory.GainCrystals(crystals);
            }
            else
            {
                base.Update(blackboard);
                ui.origin = bounds.Origin;
                ui.Update(blackboard.inputState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.Draw(spriteBatch, blackboard);
            Vector2 weaponTexHalfSize = weapon.texture.Size() / 2;
            spriteBatch.Draw(weapon.texture, bounds.TopCenter + new Vector2(0,5) - weaponTexHalfSize, Color.White);

            spriteBatch.DrawString(Game1.font, weapon.name, bounds.BottomCenter + new Vector2(0,2), TextAlignment.CENTER, Color.Yellow);

            ui.Draw(spriteBatch);
        }
    }
}