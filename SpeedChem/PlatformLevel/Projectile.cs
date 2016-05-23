using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    public class Projectile
    {
        Texture2D texture;
        public Vector2 pos;
        Vector2 size;
        float rotation;
        Vector2 velocity;
        public bool destroyed = false;

        public Projectile(Texture2D texture, Vector2 pos, Vector2 size, Vector2 velocity)
        {
            this.texture = texture;
            this.pos = pos;
            this.size = size;
            this.velocity = velocity;

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

            foreach(PlatformObject obj in allObjects)
            {
                if(CanCollide(obj) && obj.bounds.Contains(pos))
                {
                    const float VELOCITY_TRANSFER = 0.2f;
                    // hit this thing
                    if(obj is ChemBlock)
                    {
                        ChemBlock body = (ChemBlock)obj;
                        Vector2 nailDirection = velocity;
                        nailDirection.Normalize();
                        body.Nailed(nailDirection);
                        body.velocity += velocity* VELOCITY_TRANSFER;
                        body.UpdatedVelocity();
                    }
                    destroyed = true;
                }
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
