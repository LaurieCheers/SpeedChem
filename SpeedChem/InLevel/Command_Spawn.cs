using LRCEngine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    class Command_Spawn: Command
    {
        bool triggered;
        Vectangle bounds;
        ChemicalSignature blockTemplate;

        public Command_Spawn(Vectangle bounds, ChemicalSignature blockTemplate)
        {
            triggered = false;
            this.bounds = bounds;
            this.blockTemplate = blockTemplate;
        }

        public void Run()
        {
            triggered = true;
        }

        public void Update(List<WorldObject> objects)
        {
            if (!triggered)
                return;

            foreach(WorldObject obj in objects)
            {
                if (obj.bounds.Intersects(bounds))
                    return;
            }

            triggered = false;

            int height = blockTemplate.height;
            int width = blockTemplate.width;

            float startX = bounds.CenterX - width * 32 / 2;
            float startY = bounds.CenterY - height * 32 / 2;
            float curX = startX;
            float curY = startY;

            ChemBlock firstBlock = null;

            for(int col = 0; col < width; ++col)
            {
                for(int row = 0; row < height; ++row)
                {
                    ChemicalElement c = blockTemplate[col, row];
                    if (c != ChemicalElement.NONE)
                    {
                        ChemBlock newBlock = new ChemBlock(c, Game1.textures.block, new Vector2(curX, curY), new Vector2(32, 32), c.ToColor());
                        objects.Add(newBlock);

                        if(firstBlock != null)
                        {
                            firstBlock.NailOnto(newBlock);
                        }
                        else
                        {
                            firstBlock = newBlock;
                        }
                    }

                    curY += 32;
                }

                curX += 32;
                curY = startY;
            }
        }
    }
}
