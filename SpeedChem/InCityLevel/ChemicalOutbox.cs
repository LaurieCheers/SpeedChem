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
    class ChemicalOutbox : CityObject
    {
        ChemicalSignature signature;
        int price;
        public override int inputPrice { get { return price; } }

        public ChemicalOutbox(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.outbox, template.getVector2("pos"), TextureCache.outbox.Size())
        {
            this.signature = new ChemicalSignature(template.getArray("chemical"));
            this.price = template.getInt("price");
            Init();
        }

        public ChemicalOutbox(CityLevel cityLevel, ChemicalSignature signature, int price, Vector2 pos) : base(cityLevel, TextureCache.outbox, pos, TextureCache.outbox.Size())
        {
            this.signature = signature;
            this.price = price;
            Init();
        }

        void Init()
        {
            SetPipeSocket(new Vector2(16, 48), 1);
        }

        public override bool ReceiveInput(ChemicalSignature signature, ref string errorMessage)
        {
            if (this.signature == signature)
            {
                Game1.instance.inventory.GainMoney(price, bounds.Center, cityLevel);
                didOutput = true;
                return true;
            }
            else
            {
                errorMessage = "Wrong output chemical!";
                return false;
            }
        }

        public override ChemicalSignature GetInputChemical()
        {
            return signature;
        }

        public override void Update(CityUIBlackboard blackboard)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.Draw(spriteBatch, blackboard);
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
