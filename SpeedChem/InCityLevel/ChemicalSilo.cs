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
            Vector2 pos = bounds.Origin;
            if(signature != null)
                signature.Draw(spriteBatch, new Vector2(pos.X+16-4*signature.width, pos.Y-8*signature.height), true);
            ui.Draw(spriteBatch);
            spriteBatch.DrawString(Game1.font, ""+amount, new Vector2(pos.X + 8, pos.Y - 34), Color.Black);
        }
    }
}
