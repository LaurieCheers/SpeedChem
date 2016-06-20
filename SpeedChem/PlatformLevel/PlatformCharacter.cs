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
        public bool jetting;

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
            Game1.instance.inventory.UpdateWeapons(input, this, allObjects, projectiles);

            if (jetting)
            {
                const float THRUST_MINRANGE = 16.0f;
                const float THRUST_STRENGTH = 8.0f;
                const float DAMPING = 0.94f;

                Vector2 thrust = input.MousePos - bounds.Center;
                float length = thrust.Length();
                if (length < THRUST_MINRANGE)
                    length = THRUST_MINRANGE;

                //thrust.X *= 0.85f;

                thrust *= THRUST_STRENGTH / length;
                velocity = thrust;
//                velocity *= DAMPING;

//                velocity.
            }
            else
            {
                const float ACCEL = 0.2f;
                const float DAMPINGX = 0.97f;
                const float DAMPINGY = 1.0f;
                const float BRAKES_DAMPINGX = 0.75f;
                const float JUMP_XVEL = 1.7f;
                const float JUMP_YVEL = -7.7f;
                const float JUMP_DAMPINGY = 0.5f;
                const float FLOAT_DAMPINGY = 0.75f;
                const int JUMP_GRACE_FRAMES = 5;
                const int JUMP_HELD_FRAMES = 15;

                bool isGrabbing = input.IsKeyDown(Keys.LeftShift);

                if (connected.Length > 1 && !isGrabbing)
                {
                    UnbondFromGroup();
                }

                pressingLeft = input.IsKeyDown(Keys.A);
                pressingRight = input.IsKeyDown(Keys.D);
                if (pressingLeft)
                {
                    Game1.instance.inventory.pressLeftTutorial = false;
                    velocity.X -= ACCEL;
                    if (velocity.X > 0)
                        velocity.X *= BRAKES_DAMPINGX;
                    else
                        velocity.X *= DAMPINGX;

                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
                else if (pressingRight)
                {
                    Game1.instance.inventory.pressRightTutorial = false;
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

                if (onGround != null)
                {
                    jumpGraceFrames = JUMP_GRACE_FRAMES;
                }

                if (input.WasKeyJustPressed(Keys.Space))
                {
                    jumpHeldFrames = JUMP_HELD_FRAMES;
                }

                if (jumpGraceFrames > 0 && jumpHeldFrames > 0)
                {
                    Game1.instance.inventory.pressJumpTutorial = false;
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

                const float GRAVITY = 0.35f;
                velocity.Y += GRAVITY;
            }

            RunMovement(allObjects);

            if (pos.Y > 700)
            {
                pos.Y = -30;
                if (Game1.instance.platformLevel.isDoubleFactory)
                {
                    if (pos.X > 300)
                        pos.X = 300;
                }
                else
                {
                    pos.X = 100;
                }
            }

            jetting = false;
        }

        public override void HandleColliding(PlatformObject obj, Vector2 move)
        {
            const float CLIMBSPEED = -4.0f;

            float playerKnee = obj is ChemBlock ? bounds.Bottom - 8: bounds.Top;
            float playerFeet = bounds.Bottom;
            float objTopY = obj.bounds.Top;

            if ((pressingLeft || pressingRight) && playerKnee < objTopY)
            {
                 velocity.Y = CLIMBSPEED;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Game1.instance.inventory.DrawWeapons(spriteBatch);

            if (Game1.instance.inventory.pressLeftTutorial)
            {
                spriteBatch.Draw(TextureCache.keyboard_key, new Rectangle((int)bounds.X - (8 + 24), (int)bounds.Y, 24, 24), Color.White);
                spriteBatch.DrawString(Game1.font, "A", new Vector2(bounds.X - (8 + 12), bounds.Y + 1), TextAlignment.CENTER, Color.Black);
            }

            if (Game1.instance.inventory.pressRightTutorial)
            {
                spriteBatch.Draw(TextureCache.keyboard_key, new Rectangle((int)bounds.Right + 8, (int)bounds.Y, 24, 24), Color.White);
                spriteBatch.DrawString(Game1.font, "D", new Vector2(bounds.Right + 8 + 12, bounds.Y + 1), TextAlignment.CENTER, Color.Black);
            }

            if(Game1.instance.inventory.pressJumpTutorial)
            {
                spriteBatch.Draw(TextureCache.keyboard_key, new Rectangle((int)bounds.CenterX - (58/2), (int)bounds.Y - 32, 58, 24), Color.White);
                spriteBatch.DrawString(Game1.font, "Space", new Vector2(bounds.CenterX, bounds.Y + 1 - 32), TextAlignment.CENTER, Color.Black);
            }
        }
    }
}
