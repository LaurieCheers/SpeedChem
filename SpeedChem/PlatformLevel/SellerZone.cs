﻿using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    class SellerZone : PlatformObject
    {
        ChemicalSignature signature;
        int sellPrice;
        FactoryCommandType sellAction;

        public SellerZone(ChemicalSignature signature, int sellPrice, FactoryCommandType sellAction, Vector2 pos, Vector2 size) : base(null, pos, size)
        {
            objectType = PlatformObjectType.Trigger;
            this.signature = signature;
            this.sellPrice = sellPrice;
            this.sellAction = sellAction;
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            Vectangle myBounds = bounds;

            foreach (PlatformObject obj in allObjects)
            {
                if (obj is ChemBlock && !obj.destroyed && myBounds.Intersects(obj.bounds))
                {
                    ChemBlock block = obj as ChemBlock;

                    /*if (!block.chemGrid.IsInside(myBounds))
                        continue;
                    */

                    if (signature != block.chemGrid.GetSignature())
                        continue;

                    Game1.instance.platformLevel.Record(sellAction, sellPrice);
                    block.chemGrid.DestroyAll();
                    Game1.instance.platformLevel.UpdateAnyBlocksLeft();
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
