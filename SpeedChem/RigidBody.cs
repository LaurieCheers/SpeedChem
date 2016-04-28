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
    public class RigidBody: WorldObject
    {
        public RigidBody[] connected;
        public int connectedID;
        static int nextUnusedConnectID;
        public Vector2 velocity;
        public bool onGround;

        public RigidBody(Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            connected = new RigidBody[] { this };
            connectedID = nextUnusedConnectID++;
        }

        public RigidBody(Texture2D texture, Vector2 pos, Vector2 size, Color color) : base(texture, pos, size, color)
        {
            connected = new RigidBody[] { this };
            connectedID = nextUnusedConnectID++;
        }

        public RigidBody(Texture2D texture, Vector2 pos, Vector2 size, Color color, Rectangle textureRegion) : base(texture, pos, size, color, textureRegion)
        {
            connected = new RigidBody[] { this };
            connectedID = nextUnusedConnectID++;
        }

        public bool IgnoreCollisions(WorldObject obj)
        {
            if (obj is RigidBody)
                return ((RigidBody)obj).connectedID == connectedID;

            return obj.objectType == WorldObjectType.Trigger;
        }

        public virtual void HandleColliding(WorldObject obj, Vector2 move)
        {
        }

        public bool IsBondedWith(RigidBody obj)
        {
            return connectedID == obj.connectedID;
        }

        public void UnbondFromGroup()
        {
            RigidBody[] newConnected = new RigidBody[connected.Length - 1];
            int currentIdx = 0;
            foreach (RigidBody c in connected)
            {
                if (c != this)
                {
                    newConnected[currentIdx] = c;
                    currentIdx++;
                    c.connected = newConnected;
                }
            }
            connected = new RigidBody[] { this };
            connectedID = nextUnusedConnectID++;
        }

        public void BondWith(RigidBody other)
        {
            if (other.connectedID == this.connectedID)
                return;

            RigidBody[] newConnected = new RigidBody[connected.Length + other.connected.Length];
            int currentIdx = 0;
            foreach (RigidBody c in connected)
            {
                newConnected[currentIdx] = c;
                currentIdx++;
                c.connected = newConnected;
            }
            foreach (RigidBody c in other.connected)
            {
                newConnected[currentIdx] = c;
                c.connectedID = this.connectedID;
                currentIdx++;
                c.connected = newConnected;
            }
        }

        public void RunMovement(List<WorldObject> allObjects)
        {
            Vector2 moveY = new Vector2(0, velocity.Y);
            bool newOnGround = false;
            foreach (RigidBody c in connected)
            {
                moveY = c.CheckMove(this, moveY, allObjects, ref newOnGround);
            }

            foreach (RigidBody c in connected)
            {
                c.pos += moveY;
            }

            onGround = newOnGround;

            Vector2 moveX = new Vector2(velocity.X, 0);
            foreach (RigidBody c in connected)
            {
                moveX = c.CheckMove(this, moveX, allObjects, ref newOnGround);
            }

            foreach (RigidBody c in connected)
            {
                c.pos += moveX;
                c.velocity = this.velocity;
            }
        }

        protected Vector2 CheckMove(RigidBody parentObject, Vector2 currentMove, List<WorldObject> allObjects, ref bool onGround)
        {
            const float EPSILON = 0.01f;
            Vectangle moveBoundsX = GetMoveBoundsX(currentMove);
            Vectangle moveBoundsY = GetMoveBoundsY(currentMove);

            foreach (WorldObject obj in allObjects)
            {
                if (obj == this || IgnoreCollisions(obj))
                    continue;

                Vectangle objBounds = obj.bounds;
                if (currentMove.X != 0 && moveBoundsX.Intersects(objBounds))
                {
                    // TODO: need to calculate this more precisely.
                    if (currentMove.X > 0)
                    {
                        if (moveBoundsX.Intersects(objBounds.LeftSide))
                        {
                            HandleColliding(obj, currentMove);
                            currentMove.X = objBounds.X - (pos.X + size.X + EPSILON);
                            obj.CollidedX(parentObject);
                            moveBoundsX = GetMoveBoundsX(currentMove);
                        }
                    }
                    else
                    {
                        if (moveBoundsX.Intersects(objBounds.RightSide))
                        {
                            HandleColliding(obj, currentMove);
                            currentMove.X = objBounds.MaxX + EPSILON - pos.X;
                            obj.CollidedX(parentObject);
                            moveBoundsX = GetMoveBoundsX(currentMove);
                        }
                    }
                }

                if (currentMove.Y != 0 && moveBoundsY.Intersects(objBounds))
                {
                    if (currentMove.Y > 0)
                    {
                        if (moveBoundsY.Intersects(objBounds.TopSide))
                        {
                            HandleColliding(obj, currentMove);
                            currentMove.Y = objBounds.Y - (pos.Y + size.Y + EPSILON);
                            obj.CollidedY(parentObject);
                            moveBoundsY = GetMoveBoundsY(currentMove);
                            onGround = true;
                        }
                    }
                    else
                    {
                        if (moveBoundsY.Intersects(objBounds.BottomSide))
                        {
                            HandleColliding(obj, currentMove);
                            currentMove.Y = objBounds.MaxY + EPSILON - pos.Y;
                            obj.CollidedY(parentObject);
                            moveBoundsY = GetMoveBoundsY(currentMove);
                        }
                    }
                }
            }

            return currentMove;
        }

        public void UpdatedVelocity()
        {
            connected[0].velocity = this.velocity;
        }

        public Vectangle GetMoveBoundsX(Vector2 currentMove)
        {
            float MinX = pos.X;
            float MaxX = pos.X + size.X;

            if (currentMove.X < 0)
            {
                MinX = pos.X + currentMove.X;
                MaxX = pos.X;
            }
            else
            {
                MinX = pos.X + size.X;
                MaxX = MinX + currentMove.X;
            }

            float MinY = pos.Y;
            float MaxY = pos.Y + size.Y;
            if (currentMove.Y < 0)
            {
                MinY += currentMove.Y;
            }
            else
            {
                MaxY += currentMove.Y;
            }
            return new Vectangle(MinX, MinY, MaxX - MinX, MaxY - MinY);
        }

        public Vectangle GetMoveBoundsY(Vector2 currentMove)
        {
            float MinX = pos.X;
            float MaxX = pos.X + size.X;

            if (currentMove.X < 0)
            {
                MinX += currentMove.X;
            }
            else
            {
                MaxX += currentMove.X;
            }

            float MinY;
            float MaxY;
            if (currentMove.Y < 0)
            {
                MinY = pos.Y + currentMove.Y;
                MaxY = pos.Y;
            }
            else
            {
                MinY = pos.Y + size.Y;
                MaxY = MinY + currentMove.Y;
            }
            return new Vectangle(MinX, MinY, MaxX - MinX, MaxY - MinY);
        }
    }
}
