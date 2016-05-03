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
    class ChemicalOutbox : MetaGameObject
    {
        ChemicalSignature signature;
        int price;

        public ChemicalOutbox(ChemicalSignature signature, int price, Vector2 pos) : base(Game1.textures.outbox, pos, Game1.textures.outbox.Size())
        {
            this.signature = signature;
            this.price = price;
            Init();
        }

        void Init()
        {
            SetPipeSocket(new Vector2(16, 48), 10);
        }

        public override bool ReceiveInput(ChemicalSignature signature)
        {
            if (this.signature == signature)
            {
                Game1.instance.metaGame.GainMoney(price, bounds.Center);
                return true;
            }

            return false;
        }

        public override ChemicalSignature GetInputChemical()
        {
            return signature;
        }

        public override void Update(InputState inputState, ref object selectedObject)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Vector2 pos = new Vector2(bounds.X, bounds.Y + bounds.Height);
            Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

            string text = "$" + price;
            Vector2 textSize = Game1.font.MeasureString(text);

            Vector2 signaturePos = new Vector2(
                pos.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                pos.Y + (textSize.Y - signatureSize.Y) * 0.5f
            );

            Vector2 textPos = new Vector2(
                signaturePos.X + signatureSize.X,
                pos.Y
            );

            signature.Draw(spriteBatch, signaturePos, true);

            spriteBatch.DrawString(Game1.font, "$" + price, textPos, Color.Yellow);
        }
    }
}
