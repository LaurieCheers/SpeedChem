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
    class CuttingBeam: PlatformObject
    {
        Texture2D inactiveTexture;
        Texture2D activeTexture;

        public CuttingBeam(Texture2D inactiveTexture, Texture2D activeTexture, Vector2 pos, Vector2 size, Color color) : base(inactiveTexture, pos, size, color)
        {
            this.inactiveTexture = inactiveTexture;
            this.activeTexture = activeTexture;
            objectType = PlatformObjectType.Trigger;
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
/*            foreach (WorldObject obj in allObjects)
            {
            }
            */
        }
    }
}
