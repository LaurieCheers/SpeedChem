using LRCEngine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    class Spawner: Triggerable
    {
        bool triggered;
        Vectangle bounds;
        Color[,] blockTemplate;

        public Spawner(Vectangle bounds, Color[,] blockTemplate)
        {
            triggered = false;
            this.bounds = bounds;
            this.blockTemplate = blockTemplate;
        }

        public void Trigger()
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

            int height = blockTemplate.GetLength(0);
            int width = blockTemplate.GetLength(1);

            float startX = bounds.CenterX - width * 32 / 2;
            float startY = bounds.CenterY - height * 32 / 2;
            float curX = startX;
            float curY = startY;

            ChemBlock firstBlock = null;

            for(int col = 0; col < width; ++col)
            {
                for(int row = 0; row < height; ++row)
                {
                    Color c = blockTemplate[row, col];
                    if (c != null)
                    {
                        ChemBlock newBlock = new ChemBlock(Game1.instance.blockTexture, new Vector2(curX, curY), new Vector2(32, 32), c);
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
