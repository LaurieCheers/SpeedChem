using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        STEEL,
        WILDCARD,
    };

    public static class ChemicalExtension
    {
        public static Color ToColor(this ChemicalElement e)
        {
            switch(e)
            {
                case ChemicalElement.WHITE:
                    return Color.White;
                case ChemicalElement.GREEN:
                    return Color.Green;
                case ChemicalElement.RED:
                    return Color.Red;
                case ChemicalElement.BLUE:
                    return new Color(64,128,255);
                case ChemicalElement.STEEL:
                    return new Color(178, 178, 178);
                case ChemicalElement.GLASS:
                case ChemicalElement.WILDCARD:
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
                        return TextureCache.glassIcon;
                    case ChemicalElement.WHITE:
                        return TextureCache.bubbleIcon;
                    case ChemicalElement.WILDCARD:
                        return TextureCache.wildcardIcon;
                    default:
                        return TextureCache.chemIcon;
                }
            }
            else
            {
                switch (e)
                {
                    case ChemicalElement.WHITE:
                        return TextureCache.bubbleBlock;
                    case ChemicalElement.GLASS:
                        return TextureCache.glassBlock;
                    case ChemicalElement.STEEL:
                        return TextureCache.steelBlock;
                    case ChemicalElement.WILDCARD:
                        return TextureCache.wildcardBlock;
                    default:
                        return TextureCache.block;
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

        public ChemicalSignature(JSONArray template)
        {
            this.width = template.getString(0).Length;
            this.elements = new ChemicalElement[width*template.Length];

            int Idx = 0;
            foreach (string line in template.asStrings())
            {
                Debug.Assert(line.Length == width);

                foreach(char c in line)
                {
                    this.elements[Idx] = Decode(c);
                    Idx++;
                }
            }
        }

        public ChemicalSignature(int width, ChemicalElement[] elements)
        {
            this.width = width;
            this.elements = elements;
        }

        public static ChemicalElement Decode(char c)
        {
            switch(c)
            {
                case 'W': return ChemicalElement.WHITE;
                case 'B': return ChemicalElement.BLUE;
                case 'R': return ChemicalElement.RED;
                case 'G': return ChemicalElement.GLASS;
                case 'S': return ChemicalElement.STEEL;
                case '?': return ChemicalElement.WILDCARD;
                default: return ChemicalElement.NONE;
            }
        }

        public override bool Equals(Object obj)
        {
            return obj is ChemicalSignature && this == (ChemicalSignature)obj;
        }

        public ChemicalSignature Rotate()
        {
            ChemicalElement[] newElements = new ChemicalElement[elements.Length];
            int oldWidth = width;
            int oldHeight = height;
            for(int oldX = 0; oldX < oldWidth; ++oldX)
            {
                for(int oldY = 0; oldY < oldHeight; ++oldY)
                {
                    newElements[(oldHeight - 1 - oldY) + oldX * oldHeight] = elements[oldX + oldY * oldWidth];
                }
            }
            return new ChemicalSignature(oldHeight, newElements);
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
                ChemicalElement elementA = a.elements[Idx];
                ChemicalElement elementB = b.elements[Idx];
                if(elementA != elementB && elementA != ChemicalElement.WILDCARD && elementB != ChemicalElement.WILDCARD)
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
                totalPos += kv.Key.pos - new Vector2(kv.Value.X * Game1.BLOCKSIZE, kv.Value.Y * Game1.BLOCKSIZE);
                numBlocks++;
            }
            return new Vector2(totalPos.X / numBlocks, totalPos.Y / numBlocks);
        }

        public bool IsInside(Vectangle targetArea)
        {
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                if (!targetArea.Contains(kv.Key.bounds))
                    return false;
            }
            return true;
        }

        public Vector2 GetCorrection(ChemBlock block)
        {
            Point gridPos = blocks[block];
            Vector2 origin = GetOrigin();
            return new Vector2(origin.X + gridPos.X * Game1.BLOCKSIZE - block.pos.X, origin.Y + gridPos.Y * Game1.BLOCKSIZE - block.pos.Y);
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
            Point blockOffset = new Point((int)Math.Round(blockOffsetPixels.X / Game1.BLOCKSIZE), (int)Math.Round(blockOffsetPixels.Y / Game1.BLOCKSIZE));

            if (!IsValidNailOffset(blockOffset))
                return;

            Point a_in_aPos = blocks[a];
            Point b_in_bPos = b.chemGrid != null? b.chemGrid.blocks[b]: new Point(0,0);
            bool sameGrids = blocks.ContainsKey(b);

            if (!sameGrids)
            {
                Point b_in_aPos = a_in_aPos + blockOffset;
                if (b.chemGrid == null)
                {
                    blocks[b] = b_in_aPos;
                    b.chemGrid = this;
                    a.BondWith(b);
                }
                else
                {
                    // merge the grids
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
                        a.BondWith(kv.Key);
                    }
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

        public void Split(Vectangle cuttingArea)
        {
            Vector2 origin = GetOrigin();
            bool cutAny = false;
            if (cuttingArea.Height > 0)
            {
                HashSet<Point> newHBonds = new HashSet<Point>();
                foreach (Point curPos in horizontalBonds)
                {
                    Vectangle bondArea = new Vectangle(origin.X + curPos.X * Game1.BLOCKSIZE + 25, origin.Y + curPos.Y * Game1.BLOCKSIZE + 15, 15, 3);
                    if(cuttingArea.Intersects(bondArea))
                    {
                        cutAny = true;
                    }
                    else
                    {
                        newHBonds.Add(curPos);
                    }
                }
                horizontalBonds = newHBonds;
            }

            if (cuttingArea.Width > 0)
            {
                HashSet<Point> newVBonds = new HashSet<Point>();
                foreach (Point curPos in verticalBonds)
                {
                    Vectangle bondArea = new Vectangle(origin.X + curPos.X * Game1.BLOCKSIZE + 15, origin.Y + curPos.Y * Game1.BLOCKSIZE + 25, 3, 15);
                    if(cuttingArea.Intersects(bondArea))
                    {
                        cutAny = true;
                    }
                    else
                    {
                        newVBonds.Add(curPos);
                    }
                }
                verticalBonds = newVBonds;
            }

            if (cutAny)
            {
                UpdateConnectivity();
            }
        }

        public void UpdateConnectivity()
        {
            Dictionary<Point, ChemBlock> blockMapping = new Dictionary<Point, ChemBlock>();
            HashSet<Point> inactiveBlocks = new HashSet<Point>();
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                blockMapping.Add(kv.Value, kv.Key);
                inactiveBlocks.Add(kv.Value);
            }

            //now flood fill regions until everything is filled
            while (inactiveBlocks.Count > 0)
            {
                Point originPoint = inactiveBlocks.First();
                inactiveBlocks.Remove(originPoint);
                
                ChemBlock originBlock = blockMapping[originPoint];
                ChemGrid newGrid = new ChemGrid(originBlock);
                originBlock.chemGrid = newGrid;
                originBlock.UnbondFromGroup();

                List<Point> activeBlocks = new List<Point>() { originPoint };
                List<Point> nextActiveBlocks = new List<Point>();
                List<Point> finalPositions = new List<Point>();
                while (activeBlocks.Count > 0)
                {
                    foreach (Point p in activeBlocks)
                    {
                        Func<Point, bool> addBlock = pos =>
                        {
                            inactiveBlocks.Remove(pos);
                            nextActiveBlocks.Add(pos);
                            ChemBlock block = blockMapping[pos];
                            block.UnbondFromGroup();
                            block.chemGrid = null;
                            newGrid.AddNail(blockMapping[p], block);
                            return false;
                        };

                        Point up = new Point(p.X, p.Y - 1);
                        Point down = new Point(p.X, p.Y + 1);
                        Point left = new Point(p.X - 1, p.Y);
                        Point right = new Point(p.X + 1, p.Y);

                        if (verticalBonds.Contains(up) && inactiveBlocks.Contains(up))
                        {
                            addBlock(up);
                        }
                        if (verticalBonds.Contains(p) && inactiveBlocks.Contains(down))
                        {
                            addBlock(down);
                        }
                        if (horizontalBonds.Contains(left) && inactiveBlocks.Contains(left))
                        {
                            addBlock(left);
                        }
                        if (horizontalBonds.Contains(p) && inactiveBlocks.Contains(right))
                        {
                            addBlock(right);
                        }
                    }

                    List<Point> temp = activeBlocks;
                    temp.Clear();
                    activeBlocks = nextActiveBlocks;
                    nextActiveBlocks = temp;
                }

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
                spriteBatch.Draw(TextureCache.white, new Rectangle(
                    origin.X + curPos.X * (int)Game1.BLOCKSIZE + 25,
                    origin.Y + curPos.Y * (int)Game1.BLOCKSIZE + 15,
                    15,
                    3),
                    Color.Orange
                );
            }
            foreach (Point curPos in verticalBonds)
            {
                spriteBatch.Draw(TextureCache.white, new Rectangle(
                    origin.X + curPos.X * (int)Game1.BLOCKSIZE + 15,
                    origin.Y + curPos.Y * (int)Game1.BLOCKSIZE + 25,
                    3,
                    15),
                    Color.Orange
                );
            }
        }

        public void DestroyAll()
        {
            foreach (KeyValuePair<ChemBlock, Point> kv in blocks)
            {
                kv.Key.destroyed = true;
            }
        }

        public void Destroy(ChemBlock block)
        {
            Point p = blocks[block];
            
            horizontalBonds.Remove(p);
            verticalBonds.Remove(p);
            horizontalBonds.Remove(new Point(p.X+1,p.Y));
            verticalBonds.Remove(new Point(p.X, p.Y+1));

            blocks.Remove(block);

            UpdateConnectivity();
        }

        public void DoOutput()
        {
            ChemicalSignature signature = GetSignature();
            DestroyAll();

            Game1.instance.platformLevel.ProduceChemical(signature);
            Game1.instance.platformLevel.UpdateAnyBlocksLeft();
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

        public ChemBlock(ChemicalElement element, Texture2D texture, Vector2 pos, Vector2 size, Color color) : base(texture, pos, size, color, PlatformObjectType.Pushable)
        {
            this.element = element;
            chemGrid = new ChemGrid(this);
        }

        public override void Update(InputState input, List<PlatformObject> allObjects, List<Projectile> projectiles)
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

                    foreach (PlatformObject obj in allObjects)
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

        public RigidBody GetPrimaryBlock()
        {
            return connected[0];
        }

        public override void CollidedX(RigidBody other)
        {
            bool canPush = (other.onGround != null);

            if(other is ChemBlock && ((ChemBlock)other).chemGrid != this.chemGrid)
                canPush = true;

            if (other is PlatformCharacter && ((PlatformCharacter)other).jetting)
                canPush = true;

            if (canPush)
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

        public override void CollidedY(RigidBody other)
        {
            if (other is ChemBlock || (other is PlatformCharacter && ((PlatformCharacter)other).jetting))
            {
                velocity.Y = other.velocity.Y;
                other.velocity.Y *= 0.95f;
                UpdatedVelocity();
                other.UpdatedVelocity();
            }
            else
            {
                base.CollidedY(other);
            }
        }

        public void Nailed(Vector2 direction)
        {
            if(element.ShouldShatter())
            {
                destroyed = true;
                chemGrid.Destroy(this);
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

        public override void HandleColliding(PlatformObject obj, Vector2 move)
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
            chemGrid.AddNail(this, other);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this != connected[0])
                return;

            chemGrid.Draw(spriteBatch);
        }

        public void BaseDraw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
