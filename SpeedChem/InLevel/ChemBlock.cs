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
    public enum ChemicalElement
    {
        NONE,
        WHITE,
        GREEN,
        RED,
        BLUE,
        GLASS,
    };

    public static class ChemicalExtension
    {
        public static Color ToColor(this ChemicalElement e)
        {
            switch(e)
            {
                case ChemicalElement.WHITE:
                    return Color.Pink;
                case ChemicalElement.GREEN:
                    return Color.Green;
                case ChemicalElement.RED:
                    return Color.Red;
                case ChemicalElement.BLUE:
                    return Color.Blue;
                case ChemicalElement.GLASS:
                    return Color.White;
                default:
                    return Color.Black;
            }
        }

        public static Texture2D ToTexture(this ChemicalElement e, bool icon)
        {
            if (icon)
            {
                switch (e)
                {
                    case ChemicalElement.GLASS:
                        return Game1.textures.glassIcon;
                    default:
                        return Game1.textures.chemIcon;
                }
            }
            else
            {
                switch (e)
                {
                    case ChemicalElement.GLASS:
                        return Game1.textures.glassBlock;
                    default:
                        return Game1.textures.block;
                }
            }
        }

        public static bool ShouldShatter(this ChemicalElement e)
        {
            return e == ChemicalElement.GLASS;
        }
    }

    public class ChemicalSignature
    {
        public readonly int width;
        public int height { get { return elements.Length / width; } }
        readonly ChemicalElement[] elements;

        public ChemicalSignature(int width, ChemicalElement[] elements)
        {
            this.width = width;
            this.elements = elements;
        }

        public override bool Equals(Object obj)
        {
            return obj is ChemicalSignature && this == (ChemicalSignature)obj;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach(ChemicalElement e in elements)
            {
                hash = (6*hash) + e.GetHashCode();
            }
            hash ^= width.GetHashCode();
            return hash;
        }

        public static bool operator== (ChemicalSignature a, ChemicalSignature b)
        {
            bool aNull = object.ReferenceEquals(a, null);
            bool bNull = object.ReferenceEquals(b, null);

            if (aNull)
                return bNull;
            else if (bNull)
                return false;

            if (a.width != b.width || a.elements.Length != b.elements.Length)
                return false;

            for(int Idx = 0; Idx < a.elements.Length; ++Idx)
            {
                if (a.elements[Idx] != b.elements[Idx])
                    return false;
            }
            return true;
        }

        public static bool operator !=(ChemicalSignature a, ChemicalSignature b)
        {
            return !(a == b);
        }

        public ChemicalElement this[int x, int y]
        {
            get {
                return elements[x+y*width];
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos, bool icon)
        {
            int y = 0;
            for(int Idx = 0; Idx < elements.Length;)
            {
                for (int x = 0; x < width; x++)
                {
                    ChemicalElement element = elements[Idx];
                    if (element != ChemicalElement.NONE)
                    {
                        Texture2D texture = element.ToTexture(icon);
                        spriteBatch.Draw(texture, new Rectangle((int)pos.X + x*texture.Width, (int)pos.Y + y*texture.Height, texture.Width, texture.Height), element.ToColor());
                    }
                    Idx++;
                }
                y++;
            }
        }
    }

    class ChemGrid
    {
        HashSet<Point> horizontalBonds;
        HashSet<Point> verticalBonds;
        Dictionary<ChemBlock, Point> blocks;

        public ChemGrid(ChemBlock solo)
        {
            blocks = new Dictionary<ChemBlock, Point> { { solo, Point.Zero } };
            horizontalBonds = new HashSet<Point>();
            verticalBonds = new HashSet<Point>();
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
                spriteBatch.Draw(Game1.textures.white, new Rectangle(
                    origin.X + curPos.X * 32 + 25,
                    origin.Y + curPos.Y * 32 + 15,
                    15,
                    3),
                    Color.Orange
                );
            }
            foreach (Point curPos in verticalBonds)
            {
                spriteBatch.Draw(Game1.textures.white, new Rectangle(
                    origin.X + curPos.X * 32 + 15,
                    origin.Y + curPos.Y * 32 + 25,
                    3,
                    15),
                    Color.Orange
                );
            }
        }

        public void DoOutput()
        {
            ChemicalSignature signature = GetSignature();

            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                kv.Key.destroyed = true;
            }

            Game1.instance.level.ProduceChemical(signature);
            Game1.instance.level.UpdateSaveButton();
        }

        public ChemicalSignature GetSignature()
        {
            Point p = blocks.First().Value;
            int minX = p.X;
            int minY = p.Y;
            int maxX = p.X;
            int maxY = p.Y;
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                minX = Math.Min(minX, kv.Value.X);
                minY = Math.Min(minY, kv.Value.Y);
                maxX = Math.Max(maxX, kv.Value.X);
                maxY = Math.Max(maxY, kv.Value.Y);
            }

            int width = maxX + 1 - minX;
            int height = maxY + 1 - minY;
            int signatureLength = width * height;
            ChemicalElement[] signature = new ChemicalElement[signatureLength];
            for(int Idx = 0; Idx < signatureLength; ++Idx)
            {
                signature[Idx] = ChemicalElement.NONE;
            }

            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                signature[(kv.Value.X-minX) + (kv.Value.Y - minY) * width] = kv.Key.element;
            }

            return new ChemicalSignature(width, signature);
        }
    }

    class ChemBlock : RigidBody
    {
        Vector2 nailDirection;
        int nailDuration;
        public readonly ChemicalElement element;
        public ChemGrid chemGrid;
        const int NAIL_DURATION_MAX = 20;
        const float NAIL_SEARCH_RANGE = 4.0f;
        const float NAIL_SEARCH_NARROW = 8.0f;

        public ChemBlock(ChemicalElement element, Texture2D texture, Vector2 pos, Vector2 size, Color color) : base(texture, pos, size, color)
        {
            this.element = element;
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
                        if(obj != this && obj is ChemBlock && checkArea.Intersects(obj.bounds))
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
            if(element.ShouldShatter())
            {
                destroyed = true;
                return;
            }

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
