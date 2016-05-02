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
        ChemicalFactory factory;
        int inputIndex;

        public Command_Spawn(Vectangle bounds, ChemicalFactory factory, int inputIndex)
        {
            triggered = false;
            this.bounds = bounds;
            this.factory = factory;
            this.inputIndex = inputIndex;
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

            ChemicalSignature signature = factory.ConsumeInput(inputIndex);
            if (signature == null)
                return;

            triggered = false;

            int height = signature.height;
            int width = signature.width;

            float startX = bounds.CenterX - width * 32 / 2;
            float startY = bounds.CenterY - height * 32 / 2;
            float curX = startX;
            float curY = startY;

            ChemBlock[,] blocksSpawned = new ChemBlock[width, height];

            for(int col = 0; col < width; ++col)
            {
                for(int row = 0; row < height; ++row)
                {
                    ChemicalElement c = signature[col, row];
                    if (c != ChemicalElement.NONE)
                    {
                        ChemBlock newBlock = new ChemBlock(c, Game1.textures.block, new Vector2(curX, curY), new Vector2(32, 32), c.ToColor());
                        objects.Add(newBlock);

                        blocksSpawned[col, row] = newBlock;

                        if(col > 0 && blocksSpawned[col-1,row] != null)
                        {
                            blocksSpawned[col - 1, row].NailOnto(newBlock);
                        }

                        if (row > 0 && blocksSpawned[col, row - 1] != null)
                        {
                            blocksSpawned[col, row - 1].NailOnto(newBlock);
                        }
                    }

                    curY += 32;
                }

                curX += 32;
                curY = startY;

                Game1.instance.level.UpdateSaveButton();
            }
        }
    }
}
