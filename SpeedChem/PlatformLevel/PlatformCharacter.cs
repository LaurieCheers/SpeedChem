using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    public class PlatformCharacter: RigidBody
    {
        Weapon weapon = new Weapon_Rivetgun();

        public PlatformCharacter(Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            objectType = PlatformObjectType.Character;
        }

        public PlatformCharacter(Texture2D texture, Vector2 pos, Vector2 size, Color color, Rectangle textureRegion) : base(texture, pos, size, color, textureRegion)
        {
            objectType = PlatformObjectType.Character;
        }

        public void SelectWeapon(Weapon weapon)
        {
            this.weapon = weapon;
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            const float ACCEL = 0.2f;
            const float DAMPINGX = 0.97f;
            const float DAMPINGY = 1.0f;
            const float BRAKES_DAMPINGX = 0.75f;
            const float GRAVITY = 0.35f;
            const float JUMP_XVEL = 1.7f;
            const float JUMP_YVEL = -7.7f;
            const float JUMP_DAMPINGY = 0.5f;
            const float FLOAT_DAMPINGY = 0.75f;

            bool isGrabbing = input.IsKeyDown(Keys.LeftShift);

            if(connected.Length > 1 && !isGrabbing)
            {
                UnbondFromGroup();
            }

            if (input.IsKeyDown(Keys.A))
            {
                velocity.X -= ACCEL;
                if (velocity.X > 0)
                    velocity.X *= BRAKES_DAMPINGX;
                else
                    velocity.X *= DAMPINGX;

                if(!isGrabbing)
                    spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else if (input.IsKeyDown(Keys.D))
            {
                velocity.X += ACCEL;
                if (velocity.X < 0)
                    velocity.X *= BRAKES_DAMPINGX;
                else
                    velocity.X *= DAMPINGX;

                if (!isGrabbing)
                    spriteEffects = SpriteEffects.None;
            }
            else
            {
                velocity.X *= BRAKES_DAMPINGX;
            }

            if (velocity.Y > 0 || input.IsKeyDown(Keys.Space))
            {
                velocity.Y *= DAMPINGY;
            }
            else
            {
                velocity.Y *= JUMP_DAMPINGY;
            }

            if (onGround && input.WasKeyJustPressed(Keys.Space))
            {
                if (input.IsKeyDown(Keys.A))
                    velocity.X -= JUMP_XVEL;
                else if (input.IsKeyDown(Keys.D))
                    velocity.X += JUMP_XVEL;
                velocity.Y = JUMP_YVEL;
            }

            if(input.WasKeyJustPressed(Keys.LeftShift))
            {
                Vectangle grabZone = GetGrabZone();
                foreach(PlatformObject obj in allObjects)
                {
                    if(obj is ChemBlock && grabZone.Intersects(obj.bounds))
                    {
                        BondWith((ChemBlock)obj);
                        break;
                    }
                }
            }

            weapon.Update(input, this, allObjects, projectiles);

            velocity.Y += GRAVITY;

            RunMovement(allObjects);
        }

        Vectangle GetGrabZone()
        {
            if (spriteEffects == SpriteEffects.None)
            {
                // face right
                return new Vectangle(pos.X + size.X, pos.Y, size.X / 2, size.Y / 2);
            }
            else
            {
                // face left
                return new Vectangle(pos.X - size.X / 2, pos.Y, size.X / 2, size.Y / 2);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            weapon.Draw(spriteBatch);
        }
    }
}
