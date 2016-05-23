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
    class PushButton: PlatformObject
    {
        bool isPushed = false;
        Texture2D pushedTexture;
        Texture2D unpushedTexture;
        Command target;

        public PushButton(Command target, Texture2D pushedTexture, Texture2D unpushedTexture, Vector2 pos, Vector2 size): base(unpushedTexture, pos, size)
        {
            this.target = target;
            this.pushedTexture = pushedTexture;
            this.unpushedTexture = unpushedTexture;
            objectType = PlatformObjectType.Trigger;
        }

        public PushButton(Command target, Texture2D pushedTexture, Texture2D unpushedTexture, Vector2 pos, Vector2 size, Color color) : base(unpushedTexture, pos, size, color)
        {
            this.target = target;
            this.pushedTexture = pushedTexture;
            this.unpushedTexture = unpushedTexture;
            objectType = PlatformObjectType.Trigger;
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            bool wasPushed = isPushed;
            isPushed = false;
            Vectangle myBounds = bounds;

            foreach (PlatformObject obj in allObjects)
            {
                if(obj is RigidBody)
                {
                    if (obj.bounds.Intersects(myBounds))
                    {
                        isPushed = true;
                        break;
                    }
                }
            }

            if (!isPushed)
            {
                const int EXPAND = 8;
                Vectangle expandedBounds = myBounds;
                expandedBounds.X -= EXPAND;
                expandedBounds.Y -= EXPAND;
                expandedBounds.Width += EXPAND*2;
                expandedBounds.Height += EXPAND*2;
                foreach (Projectile p in projectiles)
                {
                    if(expandedBounds.Contains(p.pos))
                    {
                        isPushed = true;
                        break;
                    }
                }
            }

            if (isPushed)
            {
                texture = pushedTexture;
                if (!wasPushed)
                    target.Run();
            }
            else
            {
                texture = unpushedTexture;
            }
        }

        public override void CollidedX(RigidBody other)
        {
        }

        public override void CollidedY(RigidBody other)
        {
        }
    }
}
