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
    public class PipeSocket
    {
        public readonly MetaGameObject parent;
        public readonly Vector2 offset;
        int maxConnections;
        public HashSet<OutputPipe> connectedPipes = new HashSet<OutputPipe>();

        public PipeSocket(MetaGameObject parent, Vector2 offset, int maxConnections)
        {
            this.parent = parent;
            this.offset = offset;
            this.maxConnections = maxConnections;
        }

        public void RemoveConnection(OutputPipe pipe)
        {
            if (!connectedPipes.Contains(pipe))
                return;

            pipe.ConnectTo(null);
            connectedPipes.Remove(pipe);
        }

        public bool AddConnection(OutputPipe newPipe)
        {
            if (connectedPipes.Contains(newPipe))
                return false;

            if (maxConnections <= connectedPipes.Count)
                return false;

            foreach (OutputPipe pipe in connectedPipes)
            {
                if (pipe.source == newPipe.source)
                    return false;
            }

            connectedPipes.Add(newPipe);
            return true;
        }

        public bool IsConnectedTo(OutputPipe pipe)
        {
            return connectedPipes.Contains(pipe);
        }

        public Vector2 pos {
            get
            {
                return parent.bounds.Origin + offset;
            }
        }
    }

    public class OutputPipe
    {
        public MetaGameObject source;
        public Vector2 sourceOffset { get; private set;  }
        public PipeSocket connectedTo { get; private set; }

        Vector2 cachedSource;
        Vector2 cachedOffset;
        float cachedRotation;
        int cachedLength;
        Rectangle cachedHandleRect;
        SpriteEffects cachedEffects;

        float animatingPip;

        bool dragging;

        public OutputPipe(MetaGameObject source, Vector2 offset)
        {
            this.source = source;
            this.sourceOffset = offset;
            ShowDisconnected();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 sourcePos = this.sourcePos;

            if (connectedTo != null)
                UpdateForTargetPos(connectedTo.pos);
            else
                UpdateForTargetPos(sourcePos + cachedOffset);

            spriteBatch.Draw(Game1.textures.pipe, new Rectangle((int)sourcePos.X, (int)sourcePos.Y, cachedLength, 16),
                null, Color.White, cachedRotation, new Vector2(0, 8), cachedEffects, 0.0f);

            if(animatingPip > 0.0f)
            {
                Vector2 targetOffset = cachedOffset;
                targetOffset.Normalize();
                targetOffset *= animatingPip;

                spriteBatch.Draw(Game1.textures.chemIcon, new Rectangle((int)(sourcePos.X + targetOffset.X - 2), (int)(sourcePos.Y + targetOffset.Y - 2), 4, 4), Color.Yellow);
            }

            spriteBatch.Draw(Game1.textures.pipeHandle, cachedHandleRect, Color.White);
        }

        void UpdateForTargetPos(Vector2 targetPos)
        {
            Vector2 offset = targetPos - sourcePos;
            if (offset != cachedOffset || sourcePos != cachedSource)
            {
                float length = offset.Length();
                Vector2 dir = offset / length;
                cachedOffset = offset;
                cachedLength = (int)length;
                cachedRotation = offset.ToAngle();
                Vector2 handlePos;
                if (length < 64.0f)
                    handlePos = sourcePos + dir * 32.0f;
                else
                    handlePos = targetPos - dir * 32.0f;
                cachedHandleRect = new Rectangle((int)handlePos.X - 8, (int)handlePos.Y - 12, 16, 16);
                cachedEffects = offset.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }
        }

        public Vector2 sourcePos { get { return source.bounds.Origin + sourceOffset; } }

        public void ConnectTo(PipeSocket newSocket)
        {
            if ((newSocket == null || newSocket.parent != source) && connectedTo != newSocket)
            {
                PipeSocket oldConnection = connectedTo;
                connectedTo = newSocket;

                if (oldConnection != null)
                {
                    oldConnection.RemoveConnection(this);
                }

                if (newSocket != null)
                {
                    if(!newSocket.AddConnection(this))
                    {
                        connectedTo = null;
                        ShowDisconnected();
                    }
                }
            }
        }

        void ShowDisconnected()
        {
            UpdateForTargetPos(sourcePos + new Vector2(0, 32));
        }

        public void Update(InputState inputState, MetaGame metaGame, ref object selectedObject)
        {
            const float PIPSPEED = 5.0f;
            if(animatingPip > 0.0f)
            {
                animatingPip += PIPSPEED;
                if(animatingPip >= cachedLength)
                {
                    animatingPip = 0.0f;
                }
            }

            bool inBounds = cachedHandleRect.Contains(inputState.MousePos);
            if (inBounds && inputState.WasMouseLeftJustPressed())
            {
                dragging = true;
            }

            if(dragging)
            {
                ConnectTo(null);
                if (inputState.mouseLeft.pressed)
                {
                    MetaGameObject overObject = metaGame.GetObjectAt(inputState.MousePos);
                    if (overObject != null && overObject.pipeSocket != null)
                    {
                        UpdateForTargetPos(overObject.pipeSocket.pos);
                    }
                    else
                    {
                        UpdateForTargetPos(inputState.MousePos);
                    }
                }
                else
                {
                    dragging = false;
                    MetaGameObject overObject = metaGame.GetObjectAt(inputState.MousePos);
                    if (overObject != null && overObject != source && overObject.pipeSocket != null)
                    {
                        ConnectTo(overObject.pipeSocket);
                    }
                    else
                    {
                        ShowDisconnected();
                    }
                }
            }
        }

        public void AnimatePip()
        {
            animatingPip = 0.01f;
        }
    }
}
