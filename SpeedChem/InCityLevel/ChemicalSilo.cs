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
    public class ChemicalSilo: CityObject
    {
        UIContainer ui;
        ChemicalSignature signature;
        public int amount;
        int maxAmount = 999;

        public ChemicalSilo(CityLevel cityLevel, JSONTable template): base(
            cityLevel,
            template.getBool("movable", true)? TextureCache.silo : TextureCache.grassy_silo,
            template.getVector2("pos"),
            TextureCache.silo.Size()
          )
        {
            this.signature = new ChemicalSignature(template.getArray("chemical"));
            canDrag = template.getBool("movable", true);
            Init();
        }

        public ChemicalSilo(CityLevel cityLevel, Vector2 pos) : base(cityLevel, TextureCache.silo, pos, TextureCache.silo.Size())
        {
            Init();
        }

        public ChemicalSilo(CityLevel cityLevel, ChemicalSignature signature, int amount, Vector2 pos): base(cityLevel, TextureCache.silo, pos, TextureCache.silo.Size())
        {
            this.signature = signature;
            this.amount = amount;
            Init();
        }

        void Init()
        {
            ui = new UIContainer();

            SetPipeSocket(new Vector2(16, 16), 1);
            AddOutputPipe(new Vector2(16,16));
            unlimitedPipes = true;
        }

        public override ChemicalSignature GetInputChemical()
        {
            return signature;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            if(signature != null)
                return signature;

            foreach (OutputPipe pipe in pipeSocket.connectedPipes)
            {
                ChemicalSignature tryChemical = pipe.source.GetOutputChemical();
                if (tryChemical != null)
                {
                    signature = tryChemical;
                    return signature;
                }
            }

            return null;
        }

        public override bool ReceiveInput(ChemicalSignature signature, ref string errorMessage)
        {
            if (this.signature == null)
            {
                this.signature = signature;
                amount = 1;
                return true;
            }
            else if (this.signature == signature)
            {
                if(amount < maxAmount)
                {
                    amount++;
                    return true;
                }
                else
                {
                    errorMessage = "Silo is full";
                    return false;
                }
            }
            else
            {
                errorMessage = "Wrong output chemical!";
                return false;
            }
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe, ref string errorMessage)
        {
            if (amount > 0)
            {
                amount--;
                pipe.AnimatePip();
                return signature;
            }

            foreach (OutputPipe pipe2 in pipeSocket.connectedPipes)
            {
                string unused = "";
                ChemicalSignature result = pipe2.source.RequestOutput(pipe, ref unused);
                if(result != null)
                {
                    pipe.AnimatePip();
                    pipe2.AnimatePip();
                    return result;
                }
            }

            errorMessage = "Silo is empty";
            return null;
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.Draw(spriteBatch, blackboard);

            Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

            string text = "" + amount;
            Vector2 textSize = Game1.font.MeasureString(text);

            Vector2 signaturePos = new Vector2(
                bounds.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                bounds.Y + (textSize.Y - signatureSize.Y) * 0.5f - textSize.Y
            );

            Vector2 textPos = new Vector2(
                signaturePos.X + signatureSize.X + 2,
                bounds.Y - textSize.Y
            );

            signature.Draw(spriteBatch, signaturePos, true);
            spriteBatch.DrawString(Game1.font, text, textPos, Color.Yellow);
        }
    }
}
