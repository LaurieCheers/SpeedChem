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
    public interface Weapon
    {
        void Update(InputState input, PlatformCharacter shooter, List<PlatformObject> allObjects, List<Projectile> projectiles);
        void Draw(SpriteBatch spriteBatch);
    }

    abstract class Weapon_Projectile : Weapon
    {
        public void Update(InputState input, PlatformCharacter shooter, List<PlatformObject> allObjects, List<Projectile> projectiles)
        {
            if (input.WasMouseRightJustReleased())
            {
                Vector2 shootPos = shooter.bounds.Center;
                Vector2 targetPos = input.MousePos;
                Vector2 shootDir = (targetPos - shootPos);
                shootDir.Normalize();
                ShootProjectile(shooter, shootDir, projectiles);
            }
        }
        public abstract void ShootProjectile(PlatformCharacter shooter, Vector2 shootDir, List<Projectile> projectiles);

        public void Draw(SpriteBatch spriteBatch)
        {
        }
    }

    class Weapon_Rivetgun : Weapon_Projectile
    {
        public override void ShootProjectile(PlatformCharacter shooter, Vector2 shootDir, List<Projectile> projectiles)
        {
            const float SHOTSPEED = 10.0f;
            projectiles.Add(new Projectile(Game1.textures.white, shooter.bounds.Center, new Vector2(15, 3), shootDir * SHOTSPEED, ProjectileAction.RIVET));

            const float RECOILSPEED = 0.0f;// 2.5f;
            shooter.velocity -= shootDir * RECOILSPEED;
        }
    }

    class Weapon_BubbleGun : Weapon_Projectile
    {
        public override void ShootProjectile(PlatformCharacter shooter, Vector2 shootDir, List<Projectile> projectiles)
        {
            Game1.instance.platformLevel.Record(FactoryCommandType.SPENDCRYSTAL);

            const float SHOTSPEED = 10.0f;
            projectiles.Add(new Projectile(Game1.textures.white, shooter.bounds.Center, new Vector2(15, 3), shootDir * SHOTSPEED, ProjectileAction.BUBBLE));
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

                if (beamTraceOffset.X == 0)
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
                else if (beamTraceOffset.Y > 0)
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
            foreach (PlatformObject obj in objects)
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
                spriteBatch.Draw(Game1.textures.cuttingBeam, new Rectangle((int)beamSource.X, (int)beamSource.Y, (int)BEAMLENGTH, (int)BEAMSPACING), null, Color.White, beamAngle, new Vector2(0, 16), SpriteEffects.None, 0);
            }

            if (beamTraceDuration > 0)
            {
                spriteBatch.Draw(Game1.textures.cuttingBeam, new Rectangle((int)beamTraceOrigin.X, (int)beamTraceOrigin.Y, (int)BEAMLENGTH, (int)BEAMSPACING), null, Color.Yellow, beamTraceAngle, new Vector2(0, 16), SpriteEffects.None, 0);
            }
        }
    }
}
