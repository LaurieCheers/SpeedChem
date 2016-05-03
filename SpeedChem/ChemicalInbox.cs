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
    class ChemicalInbox: MetaGameObject
    {
        ChemicalSignature signature;
        int price;

        public ChemicalInbox(ChemicalSignature signature, int price, Vector2 pos) : base(Game1.textures.inbox, pos, Game1.textures.inbox.Size())
        {
            this.signature = signature;
            this.price = price;
            Init();
        }

        void Init()
        {
            //AddPipeSocket(new Vector2(16, 24));
            AddOutputPipe(new Vector2(16, 24));
            unlimitedPipes = true;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe)
        {
            if(Game1.instance.metaGame.PayMoney(price, bounds.Center))
            {
                pipe.AnimatePip();
                return signature;
            }

            return null;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            return signature;
        }

        public override void Update(InputState inputState, ref object selectedObject)
        {
            HandleUnlimitedPipes();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

            string text = "$" + price;
            Vector2 textSize = Game1.font.MeasureString(text);

            Vector2 signaturePos = new Vector2(
                bounds.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                bounds.Y + (textSize.Y - signatureSize.Y)*0.5f - textSize.Y
            );

            Vector2 textPos = new Vector2(
                signaturePos.X + signatureSize.X,
                bounds.Y - textSize.Y
            );

            signature.Draw(spriteBatch, signaturePos);

            spriteBatch.DrawString(Game1.font, "$" + price, textPos, Color.Yellow);
        }
    }
}
