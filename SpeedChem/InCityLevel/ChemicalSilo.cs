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
        public override float ShelfRestOffset { get { return -5; } }

        public ChemicalSilo(CityLevel cityLevel, JSONTable template): base(
            cityLevel,
            TextureCache.silo,
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

            SetPipeSocket(new Vector2(16, 28), 1);
            AddOutputPipe(new Vector2(16,54));
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

            bool hovering = (blackboard.inputState != null) ? (blackboard.inputState.hoveringElement == this) : false;
            if (hovering || dragging)
            {
                spriteBatch.Draw(
                    TextureCache.silo_hover,
                    bounds,
                    new Color(0.75f, 0.75f, 0.0f, 0.5f)
                );
            }
        }

        public override void DrawUI(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            if (signature != null)
            {
                Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

                string text = "" + amount;
                Vector2 textSize = Game1.font.MeasureString(text);

                Vector2 signaturePos = new Vector2(
                    bounds.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                    bounds.Y + 32 + (textSize.Y - signatureSize.Y) * 0.5f - textSize.Y
                );

                Vector2 textPos = new Vector2(
                    signaturePos.X + signatureSize.X + 2,
                    bounds.Y + 32 - textSize.Y
                );

                float labelHeight = Math.Max(signatureSize.Y, textSize.Y);
                Vectangle labelRect = new Vectangle(signaturePos.X, signaturePos.Y + (signatureSize.Y - labelHeight) * 0.5f, signatureSize.X + textSize.X, Math.Max(signatureSize.Y, textSize.Y));
                spriteBatch.Draw(TextureCache.white, labelRect.Bloat(5, 2), new Color(0, 0, 0, 0.5f));

                signature.Draw(spriteBatch, signaturePos, true);
                spriteBatch.DrawString(Game1.font, text, textPos, Color.LightGray);
            }
        }
    }
}
