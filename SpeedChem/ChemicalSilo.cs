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
    public class ChemicalSilo: SpriteObject
    {
        UIContainer ui;
        ChemicalSignature signature;
        public int amount;

        public ChemicalSilo(ChemicalSignature signature, int amount, Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            ui = new UIContainer();
            this.signature = signature;
            this.amount = amount;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            signature.Draw(spriteBatch, new Vector2(pos.X+16-4*signature.width, pos.Y-8*signature.height));
            ui.Draw(spriteBatch);
            spriteBatch.DrawString(Game1.font, ""+amount, new Vector2(pos.X + 8, pos.Y + 34), Color.Black);
        }
    }
}
