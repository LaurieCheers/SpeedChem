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
    public class ChemicalSilo: MetaGameObject
    {
        UIContainer ui;
        ChemicalSignature signature;
        public int amount;

        public ChemicalSilo(Vector2 pos) : base(Game1.textures.silo, pos, Game1.textures.silo.Size())
        {
            Init();
        }

        public ChemicalSilo(ChemicalSignature signature, int amount, Vector2 pos): base(Game1.textures.silo, pos, Game1.textures.silo.Size())
        {
            this.signature = signature;
            this.amount = amount;
            Init();
        }

        void Init()
        {
            ui = new UIContainer();

            SetPipeSocket(new Vector2(16, 16), 10);
            AddOutputPipe(new Vector2(16,16));
            unlimitedPipes = true;
        }

        public override ChemicalSignature GetInputChemical()
        {
            return signature;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            return signature;
        }

        public override bool ReceiveInput(ChemicalSignature signature)
        {
            if (this.signature == null)
            {
                this.signature = signature;
                amount = 1;
                return true;
            }
            else if (this.signature == signature)
            {
                amount++;
                return true;
            }
            return false;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe)
        {
            if (amount > 0)
            {
                amount--;
                pipe.AnimatePip();
                return signature;
            }

            return null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Vector2 pos = bounds.Origin;
            if(signature != null)
                signature.Draw(spriteBatch, new Vector2(pos.X+16-4*signature.width, pos.Y-8*signature.height));
            ui.Draw(spriteBatch);
            spriteBatch.DrawString(Game1.font, ""+amount, new Vector2(pos.X + 8, pos.Y - 34), Color.Black);
        }
    }
}
