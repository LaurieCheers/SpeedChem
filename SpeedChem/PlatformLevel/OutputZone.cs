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
    class OutputZone: PlatformObject
    {
        ChemicalSignature signature;
        ChemicalSignature signature2;
        int sellPrice;

        public OutputZone(ChemicalSignature signature, ChemicalSignature signature2, Vector2 pos, Vector2 size) : base(null, pos, size)
        {
            objectType = PlatformObjectType.Trigger;
            this.signature = signature;
            this.signature2 = signature2;
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            Vectangle myBounds = bounds;

            foreach (PlatformObject obj in allObjects)
            {
                if (obj is ChemBlock && obj.bounds.Intersects(myBounds) && !obj.destroyed)
                {
                    ChemBlock block = (ChemBlock)obj;
                    block.chemGrid.DoOutput();

                    if (signature != null && block.chemGrid.GetSignature() != signature && (signature2 == null || block.chemGrid.GetSignature() != signature2))
                    {
                        Game1.instance.splashes.Add(new Splash("WRONG OUTPUT!", TextAlignment.CENTER, Game1.font, Color.Orange, new Vector2(300, 350), new Vector2(0, -5), 0.96f, 0, 3));
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // base.Draw(spriteBatch);

            if (signature != null)
            {
                spriteBatch.DrawString(Game1.font, "Make this:", new Vector2(bounds.CenterX, bounds.Y), TextAlignment.CENTER, Color.White);
                signature.Draw(spriteBatch, bounds.Center, false);
            }

            if (signature2 != null)
            {
                spriteBatch.DrawString(Game1.font, "And/or this:", new Vector2(bounds.CenterX, bounds.Y+100), TextAlignment.CENTER, Color.White);
                signature2.Draw(spriteBatch, bounds.Center + new Vector2(0,100), false);
            }
        }
    }
}
