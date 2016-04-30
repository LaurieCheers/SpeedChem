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
    class OutputZone: WorldObject
    {
        public OutputZone(Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            objectType = WorldObjectType.Trigger;
        }

        public override void Update(InputState input, List<WorldObject> allObjects, List<Projectile> projectiles)
        {
            Vectangle myBounds = bounds;

            foreach (WorldObject obj in allObjects)
            {
                if (obj is ChemBlock && obj.bounds.Intersects(myBounds) && !obj.destroyed)
                {
                    ChemBlock block = (ChemBlock)obj;
                    block.chemGrid.DoOutput();
                }
            }
        }
    }
}
