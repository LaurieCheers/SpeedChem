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
    class SellerZone : WorldObject
    {
        ChemicalSignature signature;
        int sellPrice;

        public SellerZone(ChemicalSignature signature, int sellPrice, Vector2 pos, Vector2 size) : base(null, pos, size)
        {
            objectType = WorldObjectType.Trigger;
            this.signature = signature;
            this.sellPrice = sellPrice;
        }

        public override void Update(InputState input, List<WorldObject> allObjects, List<Projectile> projectiles)
        {
            Vectangle myBounds = bounds;

            foreach (WorldObject obj in allObjects)
            {
                if (obj is ChemBlock && !obj.destroyed && myBounds.Intersects(obj.bounds))
                {
                    ChemBlock block = obj as ChemBlock;

                    /*if (!block.chemGrid.IsInside(myBounds))
                        continue;
                    */

                    if (signature != block.chemGrid.GetSignature())
                        continue;

                    Game1.instance.level.Record_EarnMoney(sellPrice);
                    block.chemGrid.DestroyAll();
                    Game1.instance.level.UpdateSaveButton();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                base.Draw(spriteBatch);

            if (signature != null)
            {
                spriteBatch.DrawString(Game1.font, "Make this:", new Vector2(bounds.CenterX, bounds.Y), Color.White);
                signature.Draw(spriteBatch, bounds.Center, false);
            }
        }
    }
}
