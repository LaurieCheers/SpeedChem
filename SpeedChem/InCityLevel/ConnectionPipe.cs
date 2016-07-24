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
        Rectangle cachedMouseRect;
        SpriteEffects cachedEffects;

        List<float> animatingPips = new List<float>();

        bool hovering;
        bool dragging;
        public bool movable;

        const float MOUSE_RANGE = 8;
        const float MOUSE_RANGE_SQR = MOUSE_RANGE* MOUSE_RANGE;

        public OutputPipe(CityObject source, Vector2 offset)
        {
            this.source = source;
            this.sourceOffset = offset;
            this.movable = true;
            ShowDisconnected();
        }

        public UIMouseResponder GetMouseHover(Vector2 localMousePos)
        {
            if (movable && cachedMouseRect.Contains(localMousePos))
            {
                float mouseOffsetLength = (localMousePos - cachedSource).Length();
                if (mouseOffsetLength > cachedLength)
                    mouseOffsetLength = cachedLength;
                Vector2 nearestPipePos = (cachedSource + cachedDirection * mouseOffsetLength);
                float distSqrFromNearest = (nearestPipePos - localMousePos).LengthSquared();
                if (distSqrFromNearest < MOUSE_RANGE_SQR)
                {
                    return this;
                }
            }

            return null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 sourcePos = this.sourcePos;

            if (connectedTo != null)
                UpdateForTargetPos(connectedTo.pos);
            else
                UpdateForTargetPos(sourcePos + cachedOffset);

            bool highlighted = (hovering || dragging);
            //if (movable)
            {
                spriteBatch.Draw(highlighted ? TextureCache.pipe_hover: TextureCache.pipe, new Rectangle((int)sourcePos.X, (int)sourcePos.Y, cachedLength, 16),
                    null, Color.White, cachedRotation, new Vector2(0, 8), cachedEffects, 0.0f);
            }
            /*else
            {
                int pipeTextureLength = TextureCache.grassy_pipe.Width;
                Vector2 currentPos = sourcePos;
                Vector2 step = cachedOffset*pipeTextureLength / (float)cachedLength;
                for (int lenSoFar = 0; lenSoFar < cachedLength; lenSoFar += pipeTextureLength)
                {
                    spriteBatch.Draw(TextureCache.grassy_pipe, currentPos, null, Color.White, cachedRotation, new Vector2(0, 8), 1.0f, cachedEffects, 0.0f);
                    currentPos += step;
                }
            }*/

            spriteBatch.Draw(highlighted ? TextureCache.pipe_head_hover: TextureCache.pipe_head, new Rectangle((int)sourcePos.X, (int)sourcePos.Y, 16,16), null, Color.White, cachedRotation, new Vector2(8, 8), cachedEffects, 0);
            spriteBatch.Draw(highlighted ? TextureCache.pipe_end_hover: TextureCache.pipe_end, new Rectangle((int)(sourcePos.X + cachedOffset.X), (int)(sourcePos.Y + cachedOffset.Y), 16, 16), null, Color.White, cachedRotation, new Vector2(8, 8), cachedEffects, 0);

            foreach (float animatingPip in animatingPips)
            {
                Vector2 targetOffset = cachedOffset;
                targetOffset.Normalize();
                targetOffset *= animatingPip;

                spriteBatch.Draw(TextureCache.chemIcon, new Rectangle((int)(sourcePos.X + targetOffset.X - 2), (int)(sourcePos.Y + targetOffset.Y - 2), 4, 4), Color.Yellow);
            }

            //if(movable)
            //    spriteBatch.Draw((hovering || dragging) ? TextureCache.pipeHandle_hover: TextureCache.pipeHandle, cachedHandleRect, Color.White);
        }

        void UpdateForTargetPos(Vector2 targetPos)
        {
            Vector2 offset = targetPos - sourcePos;
            if (offset != cachedOffset || sourcePos != cachedSource)
            {
                float length = offset.Length();
                Vector2 dir = offset / length;
                cachedSource = sourcePos;
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
                cachedHandleRect = new Rectangle((int)handlePos.X - 8, (int)handlePos.Y - 18, 16, 16);
                //cachedMouseRect = cachedHandleRect.Bloat(5);

                cachedMouseRect = new Rectangle((int)cachedSource.X, (int)cachedSource.Y, (int)cachedOffset.X, (int)cachedOffset.Y).FixNegatives().Bloat((int)MOUSE_RANGE);

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

            hovering = blackboard.inputState.hoveringElement == this;// cachedHandleRect.Contains(blackboard.inputState.MousePos);
            if (hovering && blackboard.inputState.WasMouseLeftJustPressed())
            {
                dragging = true;
                blackboard.selectedObject = null;
            }

            if(cachedOffset.Y < 0)
            {
                ConnectTo(null);
            }

            if(dragging)
            {
                ConnectTo(null);
                CityObject overObject = metaGame.GetObjectAt(blackboard.inputState.MousePos);
                PipeSocket overSocket = null;
                if (overObject != null)
                {
                    overSocket = overObject.GetNearestSocket(blackboard.inputState.MousePos);
                    if (overSocket != null && overSocket.pos.Y < sourcePos.Y)
                    {
                        overSocket = null;
                    }
                }

                Vector2 clampedPos = blackboard.inputState.MousePos;
                if (clampedPos.Y <= sourcePos.Y)
                    clampedPos.Y = sourcePos.Y;

                if (blackboard.inputState.mouseLeft.isDown)
                {
                    if(overSocket != null)
                    {
                        UpdateForTargetPos(overSocket.pos);
                    }
                    else
                    {
                        UpdateForTargetPos(clampedPos);
                    }
                }
                else
                {
                    dragging = false;
                    if (overSocket != null && overSocket.parent != source)
                    {
                        ConnectTo(overSocket);
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
