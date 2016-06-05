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
    class ChemicalInbox: CityObject
    {
        ChemicalSignature signature;
        int price;
        public override int outputPrice { get { return price; } }

        public ChemicalInbox(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.inbox, template.getVector2("pos"), TextureCache.inbox.Size())
        {
            this.signature = new ChemicalSignature(template.getArray("chemical"));
            this.price = template.getInt("price", 0);
            Init();
        }

        public ChemicalInbox(CityLevel cityLevel, ChemicalSignature signature, int price, Vector2 pos) : base(cityLevel, TextureCache.inbox, pos, TextureCache.inbox.Size())
        {
            this.signature = signature;
            this.price = price;
            Init();
        }

        void Init()
        {
            //AddPipeSocket(new Vector2(16, 24));
            AddOutputPipe(new Vector2(16, 20));
            unlimitedPipes = true;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe, ref string errorMessage)
        {
            if(Game1.instance.inventory.PayMoney(price, bounds.Center, cityLevel))
            {
                pipe.AnimatePip();
                return signature;
            }

            errorMessage = "Not enough money";
            return null;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            return signature;
        }

        public override void Update(CityUIBlackboard blackboard)
        {
            UpdateUnlimitedPipes();
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.Draw(spriteBatch, blackboard);
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

            signature.Draw(spriteBatch, signaturePos, true);

            if(price > 0)
               spriteBatch.DrawString(Game1.font, "$" + price, textPos, Color.Yellow);
        }
    }
}
