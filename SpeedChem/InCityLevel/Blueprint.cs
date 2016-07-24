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
    class Blueprint : CityObject
    {
        string unlocks;
        string label;
        int price;
        UIContainer ui = new UIContainer();
        bool purchased;
        public override float ShelfRestOffset { get { return 12; } }

        public Blueprint(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.blueprint, template.getVector2("pos"), TextureCache.blueprint.Size())
        {
            this.unlocks = template.getString("unlocks");
            this.price = template.getInt("price", 0);
            this.label = template.getString("label");
            this.canDrag = false;
            Init();
        }

        public Blueprint(CityLevel cityLevel, string unlocks, int price, Vector2 pos) : base(cityLevel, TextureCache.inbox, pos, TextureCache.inbox.Size())
        {
            this.unlocks = unlocks;
            this.price = price;
            this.canDrag = false;
            Init();
        }

        void Init()
        {
            if(price > 0)
                ui.Add(new UIBuyButton("Buy", price, new Rectangle(-40, 36, 112, 35), Game1.buttonStyle, button_Purchase));
            else
                ui.Add(new UIButton("Collect", new Rectangle(-40, 36, 112, 35), Game1.buttonStyle, button_Purchase));
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
                Game1.instance.inventory.AcquireBlueprint(unlocks);
                blackboard.cityLevel.RemoveObjectDeferred(this);
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

            spriteBatch.DrawString(Game1.font, label, bounds.TopCenter + new Vector2(0,-20), TextAlignment.CENTER, Color.Black);

            ui.Draw(spriteBatch);
        }
    }
}