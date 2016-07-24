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
    class CrystalOutbox : CityObject
    {
        ChemicalSignature signature;
        int numCrystals;
        public override float ShelfRestOffset { get { return 12; } }

        public CrystalOutbox(CityLevel cityLevel, JSONTable template) : base(cityLevel, TextureCache.depot, template.getVector2("pos"), TextureCache.depot.Size())
        {
            this.signature = new ChemicalSignature(template.getArray("chemical"));
            this.numCrystals = template.getInt("crystals", 1);
            Init();
        }

        public CrystalOutbox(CityLevel cityLevel, ChemicalSignature signature, Vector2 pos) : base(cityLevel, TextureCache.depot, pos, TextureCache.depot.Size())
        {
            this.signature = signature;
            Init();
        }

        void Init()
        {
            SetPipeSocket(new Vector2(16, 2), 10);
        }

        public override bool ReceiveInput(ChemicalSignature signature, ref string errorMessage)
        {
            if (this.signature == signature)
            {
                Game1.instance.inventory.GainCrystals(numCrystals);// bounds.Center);
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
        }

        public override void DrawUI(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            Vector2 pos = new Vector2(bounds.X, bounds.Y + 32);
            Vector2 signatureSize = new Vector2(signature.width * 8, signature.height * 8);

            string text = numCrystals > 1? " +"+numCrystals+" bubbles": " +1 bubble";
            Vector2 textSize = Game1.font.MeasureString(text);

            Vector2 signaturePos = new Vector2(
                pos.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                pos.Y + (textSize.Y - signatureSize.Y) * 0.5f
            );

            Vector2 textPos = new Vector2(
                signaturePos.X + signatureSize.X,
                pos.Y
            );

            Vectangle labelRect = new Vectangle(signaturePos.X, signaturePos.Y + (signatureSize.Y - textSize.Y) * 0.5f, signatureSize.X + textSize.X, Math.Max(signatureSize.Y, textSize.Y));
            spriteBatch.Draw(TextureCache.white, labelRect.Bloat(5,2), new Color(0, 0, 0, 0.5f));
            signature.Draw(spriteBatch, signaturePos, true);

            spriteBatch.DrawString(Game1.font, text, textPos, Color.Yellow);
        }
    }
}
