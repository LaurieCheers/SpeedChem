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
    class ChemGrid
    {
        HashSet<Point> horizontalBonds;
        HashSet<Point> verticalBonds;
        Dictionary<ChemBlock, Point> blocks;
        int width;
        int height;

        public ChemGrid(ChemBlock solo)
        {
            blocks = new Dictionary<ChemBlock, Point> { { solo, Point.Zero } };
            horizontalBonds = new HashSet<Point>();
            verticalBonds = new HashSet<Point>();
            width = 1;
            height = 1;
        }

        public Vector2 GetOrigin()
        {
            int numBlocks = 0;
            Vector2 totalPos = Vector2.Zero;
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                totalPos += kv.Key.pos - new Vector2(kv.Value.X * 32.0f, kv.Value.Y * 32.0f);
                numBlocks++;
            }
            return new Vector2(totalPos.X / numBlocks, totalPos.Y / numBlocks);
        }

        public Vector2 GetCorrection(ChemBlock block)
        {
            Point gridPos = blocks[block];
            Vector2 origin = GetOrigin();
            return new Vector2(origin.X + gridPos.X * 32.0f - block.pos.X, origin.Y + gridPos.Y * 32.0f - block.pos.Y);
        }

        public void AddNail(ChemBlock a, Vector2 direction)
        {
            Point aPos = blocks[a];
            Point offset = new Point((int)Math.Round(direction.X), (int)Math.Round(direction.Y));

            if (!IsValidNailOffset(offset))
                return;

            Point bPos = aPos + offset;
            bool found = false;
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                if (kv.Value == bPos)
                {
                    found = true;
                    break;
                }
            }

            // no block to connect to
            if (!found)
                return;

            if (offset.X == 1)
            {
                horizontalBonds.Add(aPos);
            }
            else if (offset.X == -1)
            {
                horizontalBonds.Add(aPos+offset);
            }
            else if (offset.Y == 1)
            {
                verticalBonds.Add(aPos);
            }
            else if (offset.Y == -1)
            {
                verticalBonds.Add(aPos + offset);
            }
        }

        bool IsValidNailOffset(Point offset)
        {
            if (offset.Y == 0 && (offset.X == 1 || offset.X == -1))
            {
                return true;
            }
            else if (offset.X == 0 && (offset.Y == 1 || offset.Y == -1))
            {
                return true;
            }
            else
            {
                // not aligned enough, can't add the nail
                return false;
            }
        }

        public void AddNail(ChemBlock a, ChemBlock b)
        {
            System.Diagnostics.Debug.Assert(a.chemGrid == this);

            Vector2 blockOffsetPixels = b.pos - a.pos;
            Point blockOffset = new Point((int)Math.Round(blockOffsetPixels.X / 32.0f), (int)Math.Round(blockOffsetPixels.Y / 32.0f));

            if (!IsValidNailOffset(blockOffset))
                return;

            Point a_in_aPos = blocks[a];
            Point b_in_bPos = b.chemGrid.blocks[b];
            bool sameGrids = blocks.ContainsKey(b);

            if (!sameGrids)
            {
                // merge the grids
                Point b_in_aPos = a_in_aPos + blockOffset;
                Point gridOffset = b_in_aPos - b_in_bPos;
                width = Math.Max(width, gridOffset.X + b.chemGrid.width);
                height = Math.Max(height, gridOffset.Y + b.chemGrid.height);
                foreach (Point p in b.chemGrid.horizontalBonds)
                {
                    horizontalBonds.Add(p + gridOffset);
                }
                foreach (Point p in b.chemGrid.verticalBonds)
                {
                    verticalBonds.Add(p + gridOffset);
                }
                foreach (KeyValuePair<ChemBlock, Point> kv in b.chemGrid.blocks)
                {
                    blocks[kv.Key] = kv.Value + gridOffset;
                    kv.Key.chemGrid = this;
                    a.BondWith(b);
                }
            }

            // they're already connected, just add the bond
            if (blockOffset.X == 0)
            {
                if (blockOffset.Y == 1)
                {
                    verticalBonds.Add(a_in_aPos);
                }
                else
                {
                    verticalBonds.Add(a_in_aPos + blockOffset);
                }
            }
            else if (blockOffset.X == 1)
            {
                horizontalBonds.Add(a_in_aPos);
            }
            else
            {
                horizontalBonds.Add(a_in_aPos + blockOffset);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (ChemBlock block in blocks.Keys)
            {
                block.BaseDraw(spriteBatch);
            }
            Point origin = GetOrigin().ToPoint();
            foreach(Point curPos in horizontalBonds)
            {
                spriteBatch.Draw(Game1.instance.whiteTexture, new Rectangle(
                    origin.X + curPos.X * 32 + 25,
                    origin.Y + curPos.Y * 32 + 15,
                    15,
                    3),
                    Color.Orange
                );
            }
            foreach (Point curPos in verticalBonds)
            {
                spriteBatch.Draw(Game1.instance.whiteTexture, new Rectangle(
                    origin.X + curPos.X * 32 + 15,
                    origin.Y + curPos.Y * 32 + 25,
                    3,
                    15),
                    Color.Orange
                );
            }
        }
    }

    class ChemBlock : RigidBody
    {
        Vector2 nailDirection;
        int nailDuration;
        public ChemGrid chemGrid;
        const int NAIL_DURATION_MAX = 20;
        const float NAIL_SEARCH_RANGE = 4.0f;
        const float NAIL_SEARCH_NARROW = 8.0f;

        public ChemBlock(Texture2D texture, Vector2 pos, Vector2 size, Color color) : base(texture, pos, size, color)
        {
            objectType = WorldObjectType.Pushable;
            chemGrid = new ChemGrid(this);
        }

        public override void Update(InputState input, List<WorldObject> allObjects, List<Projectile> projectiles)
        {
            if (nailDuration > 0)
            {
                if (nailDuration == NAIL_DURATION_MAX)
                {
                    Vectangle checkArea;
                    if (Math.Abs(nailDirection.X) > Math.Abs(nailDirection.Y))
                    {
                        checkArea = GetMoveBoundsX(nailDirection * NAIL_SEARCH_RANGE);
                        checkArea.Y += NAIL_SEARCH_NARROW;
                        checkArea.Height -= NAIL_SEARCH_NARROW * 2;
                    }
                    else
                    {
                        checkArea = GetMoveBoundsY(nailDirection * NAIL_SEARCH_RANGE);
                        checkArea.X += NAIL_SEARCH_NARROW;
                        checkArea.Width -= NAIL_SEARCH_NARROW * 2;
                    }

                    foreach (WorldObject obj in allObjects)
                    {
                        if(obj is ChemBlock && checkArea.Intersects(obj.bounds))
                        {
                            NailOnto((ChemBlock)obj);
                            nailDuration = 0;
                            break;
                        }
                    }
                }
                nailDuration--;
                if (nailDuration == 0)
                {
                    NailExpired();
                }
            }

            if (this != connected[0])
                return;

            const float DAMPINGX = 0.75f;
            const float GRAVITY = 0.35f;
            velocity.X *= DAMPINGX;
            velocity.Y += GRAVITY;

            RunMovement(allObjects);
        }

        public override void CollidedX(RigidBody other)
        {
            if (other.onGround)
            {
                velocity.X = other.velocity.X;
                other.velocity.X *= 0.95f;
                UpdatedVelocity();
                other.UpdatedVelocity();
            }
            else
            {
                base.CollidedX(other);
            }
        }

        public void Nailed(Vector2 direction)
        {
            if (nailDuration > 0)
                NailExpired();

            nailDirection = direction;
            nailDuration = NAIL_DURATION_MAX;
        }

        void NailExpired()
        {
            // TODO: check for an adjacent block in the nailDirection
            chemGrid.AddNail(this, nailDirection);
        }

        public override void HandleColliding(WorldObject obj, Vector2 move)
        {
            if (obj is ChemBlock)
            {
                ChemBlock block = (ChemBlock)obj;
                if (!IsBondedWith(block) && nailDuration > 0)
                {
                    Vector2 moveDir = move;
                    moveDir.Normalize();
                    if (nailDirection.DotProduct(moveDir) > 0.5f)
                    {
                        NailOnto(block);
                        nailDuration = 0;
                    }
                }
            }
        }

        public void NailOnto(ChemBlock other)
        {
            if (chemGrid == null)
            {
                chemGrid = new ChemGrid(this);
            }

            chemGrid.AddNail(this, other);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this != connected[0])
                return;

            if (chemGrid != null)
                chemGrid.Draw(spriteBatch);
            else
                base.Draw(spriteBatch);
        }

        public void BaseDraw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
