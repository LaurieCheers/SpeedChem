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
    class PushButton: WorldObject
    {
        bool isPushed = false;
        Texture2D pushedTexture;
        Texture2D unpushedTexture;
        Triggerable target;

        public PushButton(Triggerable target, Texture2D pushedTexture, Texture2D unpushedTexture, Vector2 pos, Vector2 size): base(unpushedTexture, pos, size)
        {
            this.target = target;
            this.pushedTexture = pushedTexture;
            this.unpushedTexture = unpushedTexture;
            objectType = WorldObjectType.Trigger;
        }

        public PushButton(Triggerable target, Texture2D pushedTexture, Texture2D unpushedTexture, Vector2 pos, Vector2 size, Color color) : base(unpushedTexture, pos, size, color)
        {
            this.target = target;
            this.pushedTexture = pushedTexture;
            this.unpushedTexture = unpushedTexture;
            objectType = WorldObjectType.Trigger;
        }

        public override void Update(InputState input, List<WorldObject> allObjects, List<Projectile> projectiles)
        {
            bool wasPushed = isPushed;
            isPushed = false;
            Vectangle myBounds = bounds;

            foreach (WorldObject obj in allObjects)
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

            if (isPushed)
            {
                texture = pushedTexture;
                if (!wasPushed)
                    target.Trigger();
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
