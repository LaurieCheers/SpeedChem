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
        bool pressingLeft;
        bool pressingRight;
        int jumpGraceFrames;
        int jumpHeldFrames;

        public PlatformCharacter(Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            objectType = PlatformObjectType.Character;
        }

        public PlatformCharacter(Texture2D texture, Vector2 pos, Vector2 size, Color color, Rectangle textureRegion) : base(texture, pos, size, color, textureRegion)
        {
            objectType = PlatformObjectType.Character;
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
            const int JUMP_GRACE_FRAMES = 5;
            const int JUMP_HELD_FRAMES = 15;

            bool isGrabbing = input.IsKeyDown(Keys.LeftShift);

            if(connected.Length > 1 && !isGrabbing)
            {
                UnbondFromGroup();
            }

            pressingLeft = input.IsKeyDown(Keys.A);
            pressingRight = input.IsKeyDown(Keys.D);
            if (pressingLeft)
            {
                velocity.X -= ACCEL;
                if (velocity.X > 0)
                    velocity.X *= BRAKES_DAMPINGX;
                else
                    velocity.X *= DAMPINGX;

                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else if (pressingRight)
            {
                velocity.X += ACCEL;
                if (velocity.X < 0)
                    velocity.X *= BRAKES_DAMPINGX;
                else
                    velocity.X *= DAMPINGX;

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

            if (onGround)
            {
                jumpGraceFrames = JUMP_GRACE_FRAMES;
            }

            if (input.WasKeyJustPressed(Keys.Space))
            {
                jumpHeldFrames = JUMP_HELD_FRAMES;
            }

            if (jumpGraceFrames > 0 && jumpHeldFrames > 0)
            {
                if (input.IsKeyDown(Keys.A))
                    velocity.X -= JUMP_XVEL;
                else if (input.IsKeyDown(Keys.D))
                    velocity.X += JUMP_XVEL;
                velocity.Y = JUMP_YVEL;
                jumpGraceFrames = 0;
                jumpHeldFrames = 0;
            }
            else
            {
                if (jumpGraceFrames > 0)
                    jumpGraceFrames--;

                if (jumpHeldFrames > 0)
                    jumpHeldFrames--;
            }

            Game1.instance.inventory.UpdateWeapons(input, this, allObjects, projectiles);

            velocity.Y += GRAVITY;

            RunMovement(allObjects);
        }

        public override void HandleColliding(PlatformObject obj, Vector2 move)
        {
            const float CLIMBSPEED = -4.0f;

            float objTopY = obj.bounds.Top;
            if (!(obj is ChemBlock) && (pressingLeft || pressingRight) && bounds.Top < objTopY && bounds.Bottom > objTopY)
            {
                 velocity.Y = CLIMBSPEED;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Game1.instance.inventory.DrawWeapons(spriteBatch);
        }
    }
}
