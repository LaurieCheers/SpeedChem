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
    class BuildingSite : CityObject
    {
        int price;
        CityObject spawn;
        UIContainer ui = new UIContainer();
        bool purchased;

        public BuildingSite(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.building_site, template.getVector2("pos"), TextureCache.building_site.Size())
        {
            this.spawn = CityObject.FromTemplate(cityLevel, template.getJSON("built"));
            this.price = template.getInt("price");
            Init();
        }

        public BuildingSite(CityLevel level, int price, CityObject spawn) : base(level, TextureCache.building_site, spawn.bounds.XY, TextureCache.building_site.Size())
        {
            this.spawn = spawn;
            this.price = price;
            Init();
        }

        void Init()
        {
            ui.Add(new UIBuyButton("Build", price, new Rectangle(-40,56, 112,35), Game1.buttonStyle, button_Purchase));
        }

        public void button_Purchase()
        {
            purchased = true;
        }

        public override UIMouseResponder GetOverlayMouseHover(Vector2 localMousePos)
        {
            return ui.GetMouseHover(localMousePos);
        }

        public override void Update(CityUIBlackboard blackboard)
        {
            if (purchased)
            {
                blackboard.cityLevel.AddObjectDeferred(spawn);
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

            ui.Draw(spriteBatch);
        }
    }
}
