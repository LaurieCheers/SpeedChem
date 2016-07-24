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
        ChemicalSignature signature;
        UIContainer ui = new UIContainer();
        bool purchased;
        public override float ShelfRestOffset { get { return 4; } }

        public BuildingSite(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.building_site, template.getVector2("pos"), TextureCache.building_site.Size())
        {
            this.price = template.getInt("price");
            this.canDrag = false;

            JSONArray signatureTemplate = template.getArray("chemical", null);
            if (signatureTemplate != null)
            {
                this.signature = new ChemicalSignature(signatureTemplate);
                this.spawn = new ChemicalInbox(cityLevel, signature, 0, bounds.XY);
            }
            else
            {
                this.spawn = CityObject.FromTemplate(cityLevel, template.getJSON("built"));
            }

            Init();
        }

        public BuildingSite(CityLevel level, int price, CityObject spawn) : base(level, TextureCache.building_site, spawn.bounds.XY, TextureCache.building_site.Size())
        {
            this.spawn = spawn;
            this.price = price;
            this.canDrag = false;
            Init();
        }

        void Init()
        {
            ui.Add(new UIBuyButton("Build", price, new Rectangle(-40,56, 112,35), Game1.buttonStyle, button_Purchase));
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

            if (signature != null)
            {
                Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

                Vector2 signaturePos = new Vector2(
                    bounds.CenterX - signatureSize.X * 0.5f,
                    bounds.CenterY - (signatureSize.Y * 0.5f + 8)
                );

                signature.Draw(spriteBatch, signaturePos, true);
            }

            ui.Draw(spriteBatch);
        }
    }
}
