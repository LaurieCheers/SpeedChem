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
    public class MetaGame
    {
        List<ChemicalSilo> silos = new List<ChemicalSilo>();
        List<ChemicalFactory> factories = new List<ChemicalFactory>();
        int money;
        UIContainer ui;
        Vector2 nextSiloPos = new Vector2(100,300);
        Dictionary<ChemicalSignature, ChemicalSilo> chemicals = new Dictionary<ChemicalSignature, ChemicalSilo>();

        public MetaGame()
        {
            factories.Add(new ChemicalFactory(Game1.textures.lab, new Vector2(32,64), new Vector2(32,64)));

            ui = new UIContainer();
        }

        public void Update(InputState inputState, bool isBackground)
        {
            foreach (ChemicalFactory factory in factories)
            {
                factory.Update(inputState, isBackground);
            }

            if(!isBackground)
                ui.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (ChemicalSilo silo in silos)
            {
                silo.Draw(spriteBatch);
            }

            foreach (ChemicalFactory factory in factories)
            {
                factory.Draw(spriteBatch);
            }

            ui.Draw(spriteBatch);
        }

        public void ProduceChemical(ChemicalSignature signature, int amount)
        {
            if(!chemicals.ContainsKey(signature))
            {
                ChemicalSilo newSilo = new ChemicalSilo(signature, amount, Game1.textures.silo, nextSiloPos, new Vector2(32, 32));
                silos.Add(newSilo);
                chemicals[signature] = newSilo;
                nextSiloPos.X += 50;
            }
            else
            {
                chemicals[signature].amount += amount;
            }
        }
    }
}
