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
        public readonly CityObject parent;
        public readonly Vector2 offset;
        int maxConnections;
        public HashSet<OutputPipe> connectedPipes = new HashSet<OutputPipe>();

        public PipeSocket(CityObject parent, Vector2 offset, int maxConnections)
        {
            this.parent = parent;
            this.offset = offset;
            this.maxConnections = maxConnections;
        }

        public PipeSocket(CityObject parent, Vector2 offset, Vector2 offset2)
        {
            this.parent = parent;
            this.offset = offset;
            // TODO: make two-socket objects work
            this.maxConnections = 2;
        }

        public void RemoveConnection(OutputPipe pipe)
        {
            if (!connectedPipes.Contains(pipe))
                return;

            pipe.ConnectTo(null);
            connectedPipes.Remove(pipe);
            parent.UpdatePipes();
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
            parent.UpdatePipes();
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

    public class OutputPipe: UIMouseResponder
    {
        public CityObject source;
        public Vector2 sourceOffset { get; private set;  }
        public PipeSocket connectedTo { get; private set; }

        Vector2 cachedSource;
        Vector2 cachedOffset;
        Vector2 cachedDirection;
        float cachedRotation;
        int cachedLength;
        Rectangle cachedHandleRect;
        SpriteEffects cachedEffects;

        List<float> animatingPips = new List<float>();

        bool dragging;
        public bool movable;

        public OutputPipe(CityObject source, Vector2 offset)
        {
            this.source = source;
            this.sourceOffset = offset;
            this.movable = true;
            ShowDisconnected();
        }

        public UIMouseResponder GetMouseHover(Vector2 localMousePos)
        {
            return movable && cachedHandleRect.Contains(localMousePos) ? this : null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 sourcePos = this.sourcePos;

            if (connectedTo != null)
                UpdateForTargetPos(connectedTo.pos);
            else
                UpdateForTargetPos(sourcePos + cachedOffset);

            if (movable)
            {
                spriteBatch.Draw(TextureCache.pipe, new Rectangle((int)sourcePos.X, (int)sourcePos.Y, cachedLength, 16),
                    null, Color.White, cachedRotation, new Vector2(0, 8), cachedEffects, 0.0f);
            }
            else
            {
                int pipeTextureLength = TextureCache.grassy_pipe.Width;
                Vector2 currentPos = sourcePos;
                Vector2 step = cachedOffset*pipeTextureLength / (float)cachedLength;
                for (int lenSoFar = 0; lenSoFar < cachedLength; lenSoFar += pipeTextureLength)
                {
                    spriteBatch.Draw(TextureCache.grassy_pipe, currentPos, null, Color.White, cachedRotation, new Vector2(0, 8), 1.0f, cachedEffects, 0.0f);
                    currentPos += step;
                }
            }

            foreach (float animatingPip in animatingPips)
            {
                Vector2 targetOffset = cachedOffset;
                targetOffset.Normalize();
                targetOffset *= animatingPip;

                spriteBatch.Draw(TextureCache.chemIcon, new Rectangle((int)(sourcePos.X + targetOffset.X - 2), (int)(sourcePos.Y + targetOffset.Y - 2), 4, 4), Color.Yellow);
            }

            if(movable)
                spriteBatch.Draw(TextureCache.pipeHandle, cachedHandleRect, Color.White);
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
                cachedDirection = cachedOffset;
                cachedDirection.Normalize();
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
                    if (!newSocket.AddConnection(this))
                    {
                        connectedTo = null;
                        ShowDisconnected();
                    }
                }
                else
                {
                    ShowDisconnected();
                }

                source.UpdatePipes();
            }
        }

        void ShowDisconnected()
        {
            UpdateForTargetPos(sourcePos + new Vector2(0, 32));
        }

        public void Run()
        {
            const float PIPSPEED = 5.0f;
            for(int Idx = animatingPips.Count-1; Idx >= 0; --Idx)
            {
                animatingPips[Idx] += PIPSPEED;
                if (animatingPips[Idx] >= cachedLength)
                {
                    animatingPips[Idx] = animatingPips.Last();
                    animatingPips.RemoveAt(animatingPips.Count - 1);
                }
            }
        }

        public void Update(CityLevel metaGame, CityUIBlackboard blackboard)
        {
            if (!movable)
                return;

            bool inBounds = blackboard.inputState.hoveringElement == this;// cachedHandleRect.Contains(blackboard.inputState.MousePos);
            if (inBounds && blackboard.inputState.WasMouseLeftJustPressed())
            {
                dragging = true;
                blackboard.selectedObject = null;
            }

            if(dragging)
            {
                ConnectTo(null);
                if (blackboard.inputState.mouseLeft.isDown)
                {
                    CityObject overObject = metaGame.GetObjectAt(blackboard.inputState.MousePos);
                    if (overObject != null && overObject.pipeSocket != null)
                    {
                        UpdateForTargetPos(overObject.pipeSocket.pos);
                    }
                    else
                    {
                        UpdateForTargetPos(blackboard.inputState.MousePos);
                    }
                }
                else
                {
                    dragging = false;
                    CityObject overObject = metaGame.GetObjectAt(blackboard.inputState.MousePos);
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
            animatingPips.Add(0.01f);
        }
    }
}
