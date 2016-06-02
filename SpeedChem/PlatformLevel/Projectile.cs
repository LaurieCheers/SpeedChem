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
    public enum ProjectileAction
    {
        RIVET,
        BUBBLE,
    }

    public class Projectile
    {
        Texture2D texture;
        public Vector2 pos;
        Vector2 size;
        float rotation;
        Vector2 velocity;
        public bool destroyed = false;
        ProjectileAction action;

        public Projectile(Texture2D texture, Vector2 pos, Vector2 size, Vector2 velocity, ProjectileAction action)
        {
            this.texture = texture;
            this.pos = pos;
            this.size = size;
            this.velocity = velocity;
            this.action = action;

            UpdateRotation();
        }

        void UpdateRotation()
        {
            if (velocity.X == 0)
            {
                rotation = (float)((velocity.Y > 0) ? Math.PI : -Math.PI);
            }
            else
            {
                Vector2 velDir = velocity;
                velDir.Normalize();
                float gradient = velDir.Y;

                rotation = (float)Math.Asin(gradient);
                if (velocity.X < 0)
                    rotation = -rotation;
            }
        }

        public void Update(List<PlatformObject> allObjects)
        {
            pos += velocity;

            Point windowSize = Game1.instance.Window.ClientBounds.Size;
            if(pos.X < 0 || pos.Y < 0 || windowSize.X < pos.X || windowSize.Y < pos.Y)
                destroyed = true;

            PlatformObject collisionObj = null;
            foreach (PlatformObject obj in allObjects)
            {
                if (CanCollide(obj) && obj.bounds.Contains(pos))
                {
                    collisionObj = obj;
                }
            }

            if (collisionObj == null)
                return;

            destroyed = true;

            const float VELOCITY_TRANSFER = 0.2f;
            // hit this thing
            switch (action)
            {
                case ProjectileAction.RIVET:
                    if (collisionObj is ChemBlock)
                    {
                        ChemBlock body = (ChemBlock)collisionObj;
                        Vector2 nailDirection = velocity;
                        nailDirection.Normalize();
                        body.Nailed(nailDirection);
                        body.velocity += velocity * VELOCITY_TRANSFER;
                        body.UpdatedVelocity();
                    }
                    break;

                case ProjectileAction.BUBBLE:
                    SpawnBubble(collisionObj, pos, GetCollisionNormal(collisionObj.bounds, pos - velocity, pos), allObjects);
                    break;
            }
        }

        Vector2 GetCollisionNormal(Vectangle bounds, Vector2 oldPos, Vector2 newPos)
        {
            Vectangle boundingBox = Vectangle.BoundingBox(oldPos, newPos);
            if(bounds.LeftSide.Intersects(boundingBox))
            {
                return new Vector2(-1, 0);
            }
            if (bounds.RightSide.Intersects(boundingBox))
            {
                return new Vector2(1, 0);
            }
            if (bounds.TopSide.Intersects(boundingBox))
            {
                return new Vector2(0, -1);
            }
            if (bounds.BottomSide.Intersects(boundingBox))
            {
                return new Vector2(0, 1);
            }
            return new Vector2(1, 0);
        }

        void SpawnBubble(PlatformObject objectHit, Vector2 pos, Vector2 direction, List<PlatformObject> allObjects)
        {
            Vectangle objectBounds = objectHit.bounds;
            if (objectHit is ChemBlock)
            {
                pos = objectBounds.Center + direction*16;
            }

            while (objectBounds.Contains(pos))
                pos += direction;

            pos += direction * 17;
            Vectangle bubbleBounds = new Vectangle(pos - new Vector2(16, 16), new Vector2(32, 32));

            bool failed = false;
            foreach(PlatformObject obj in allObjects)
            {
                if (obj.bounds.Intersects(bubbleBounds))
                {
                    failed = true;
                    break;
                }
            }

            if (!failed)
            {
                ChemicalElement c = ChemicalElement.WHITE;
                ChemBlock newBlock = new ChemBlock(c, c.ToTexture(false), bubbleBounds.TopLeft, bubbleBounds.Size, c.ToColor());
                if (objectHit is ChemBlock)
                {
                    newBlock.NailOnto((ChemBlock)objectHit);
                }
                allObjects.Add(newBlock);
            }
        }

        bool CanCollide(PlatformObject obj)
        {
            switch (obj.objectType)
            {
                case PlatformObjectType.Character:
                case PlatformObjectType.Trigger:
                    return false;
                default:
                    return true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y), null, Color.Yellow, rotation, new Vector2(size.X/2, size.Y/2), SpriteEffects.None, 0);
        }
    }
}
