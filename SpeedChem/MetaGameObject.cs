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
    public class MetaGameObject
    {
        SpriteObject sprite;
        public bool selected = false;
        bool dragging = false;
        Vector2 draggingOffset;
        public PipeSocket pipeSocket;
        public List<OutputPipe> pipes = new List<OutputPipe>();
        public bool unlimitedPipes;
        public Vectangle bounds;

        public MetaGameObject(Texture2D texture, Vector2 pos, Vector2 size)
        {
            sprite = new SpriteObject(texture, pos, size);
            sprite.layerDepth = 0.0f;
            bounds = new Vectangle(pos, size);
        }

        public void SetPipeSocket(Vector2 offset, int maxConnections)
        {
            pipeSocket = new PipeSocket(this, offset, maxConnections);
        }

        public void AddOutputPipe(Vector2 offset)
        {
            pipes.Add(new OutputPipe(this, offset));
        }

        public virtual bool ReceiveInput(ChemicalSignature signature)
        {
            return false;
        }

        public virtual ChemicalSignature RequestOutput(OutputPipe pipe)
        {
            return null;
        }

        public virtual ChemicalSignature GetInputChemical()
        {
            return null;
        }

        public virtual ChemicalSignature GetOutputChemical()
        {
            return null;
        }

        public virtual void Run()
        {

        }

        public virtual void Update(InputState inputState, ref object selectedObject)
        {
            if(inputState.WasMouseLeftJustPressed() && bounds.Contains(inputState.MousePos))
            {
                selectedObject = this;
                dragging = true;
                draggingOffset = inputState.MousePos - bounds.Origin;
            }

            if(dragging && selectedObject == this)
            {
                if(inputState.mouseLeft.pressed)
                {
                    bounds.Origin = inputState.MousePos - draggingOffset;
                    sprite.pos = bounds.Origin;
                }
                else
                {
                    dragging = false;
                }
            }
            else
            {
                dragging = false;
            }

            if(unlimitedPipes)
            {
                bool foundOneDisconnected = false;
                int Idx = 0;
                while(Idx < pipes.Count)
                {
                    OutputPipe pipe = pipes[Idx];
                    if (pipe.connectedTo == null)
                    {
                        if (foundOneDisconnected)
                        {
                            pipes.RemoveAt(Idx);
                        }
                        else
                        {
                            foundOneDisconnected = true;
                            Idx++;
                        }
                    }
                    else
                    {
                        Idx++;
                    }
                }

                if (!foundOneDisconnected)
                    AddOutputPipe(pipes.First().sourceOffset);
            }

            selected = (selectedObject == this);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
