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
    public interface Weapon
    {
        void Update(InputState input, PlatformCharacter shooter, List<PlatformObject> allObjects, List<Projectile> projectiles);
        void Draw(SpriteBatch spriteBatch);
    }

    class Weapon_Rivetgun: Weapon
    {
        public void Update(InputState input, PlatformCharacter shooter, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            if (input.WasMouseRightJustReleased())
            {
                const float NAILSPEED = 10.0f;
                const float RECOILSPEED = 0.0f;// 2.5f;
                Vector2 shootPos = shooter.bounds.Center;
                Vector2 targetPos = input.MousePos;
                Vector2 shootDir = (targetPos - shootPos);
                shootDir.Normalize();
                projectiles.Add(new Projectile(Game1.textures.white, shooter.bounds.Center, new Vector2(15, 3), shootDir * NAILSPEED));
                shooter.velocity -= shootDir * RECOILSPEED;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
        }
    }

    class Weapon_CuttingBeam : Weapon
    {
        Vector2 beamSource;
        float beamAngle;
        Vector2 beamTraceOrigin;
        Vector2 beamTraceOffset;
        float beamTraceAngle;
        int beamTraceDuration;
        const float BEAMSPACING = 32;
        const float BEAMLENGTH = 100;

        public void Update(InputState input, PlatformCharacter shooter, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            if (beamTraceDuration > 0)
                beamTraceDuration--;

            if (input.WasMouseRightJustReleased())
            {
                Game1.instance.platformLevel.Record(FactoryCommandType.SPENDCRYSTAL);

                beamTraceDuration = 20;
                beamTraceAngle = beamAngle;
                beamTraceOrigin = beamSource;

                if(beamTraceOffset.X == 0)
                {
                    Cut(new Vectangle(beamTraceOrigin + new Vector2(BEAMSPACING * 0.5f, 0), beamTraceOffset), allObjects);
                    Cut(new Vectangle(beamTraceOrigin + new Vector2(BEAMSPACING * -0.5f, 0), beamTraceOffset), allObjects);
                }
                else
                {
                    Cut(new Vectangle(beamTraceOrigin + new Vector2(0, BEAMSPACING * 0.5f), beamTraceOffset), allObjects);
                    Cut(new Vectangle(beamTraceOrigin + new Vector2(0, BEAMSPACING * -0.5f), beamTraceOffset), allObjects);
                }
            }

            if (input.mouseRight.isDown)
            {
                // preview beam
                beamSource = shooter.bounds.Center;
                beamTraceOffset = input.MousePos - beamSource;

                if (Math.Abs(beamTraceOffset.X) > Math.Abs(beamTraceOffset.Y))
                {
                    if (beamTraceOffset.X > 0)
                    {
                        beamAngle = 0;
                        beamTraceOffset = new Vector2(BEAMLENGTH, 0);
                    }
                    else
                    {
                        beamAngle = (float)Math.PI;
                        beamTraceOffset = new Vector2(-BEAMLENGTH, 0);
                    }
                }
                else if(beamTraceOffset.Y > 0)
                {
                    beamAngle = (float)Math.PI * 0.5f;
                    beamTraceOffset = new Vector2(0, BEAMLENGTH);
                }
                else
                {
                    beamAngle = (float)Math.PI * 1.5f;
                    beamTraceOffset = new Vector2(0, -BEAMLENGTH);
                }
            }
            else
            {
                beamAngle = -1;
            }
        }

        public void Cut(Vectangle area, List<PlatformObject> objects)
        {
            HashSet<ChemGrid> gridsChecked = new HashSet<ChemGrid>();
            foreach(PlatformObject obj in objects)
            {
                ChemBlock block = obj as ChemBlock;
                if (block != null && !gridsChecked.Contains(block.chemGrid) && block.bounds.Intersects(area))
                {
                    gridsChecked.Add(block.chemGrid);

                    block.chemGrid.Split(area);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (beamAngle != -1)
            {
                spriteBatch.Draw(Game1.textures.cuttingBeam, new Rectangle((int)beamSource.X, (int)beamSource.Y, (int)BEAMLENGTH, (int)BEAMSPACING), null, Color.White, beamAngle, new Vector2(0,16),SpriteEffects.None, 0);
            }

            if(beamTraceDuration > 0)
            {
                spriteBatch.Draw(Game1.textures.cuttingBeam, new Rectangle((int)beamTraceOrigin.X, (int)beamTraceOrigin.Y, (int)BEAMLENGTH, (int)BEAMSPACING), null, Color.Yellow, beamTraceAngle, new Vector2(0, 16), SpriteEffects.None, 0);
            }
        }
    }

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
