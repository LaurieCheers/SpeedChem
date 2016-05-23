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
    public class CityObject: UIMouseResponder
    {
        public CityLevel cityLevel;
        SpriteObject sprite;
        public bool selected = false;
        public bool dragging { get; private set; }
        Vector2 draggingOffset;
        public PipeSocket pipeSocket;
        public List<OutputPipe> pipes = new List<OutputPipe>();
        public bool unlimitedPipes;
        public bool canDrag = true;
        public bool didOutput = false;
        public Vectangle bounds;

        public CityObject(CityLevel cityLevel, Texture2D texture, Vector2 pos, Vector2 size)
        {
            this.cityLevel = cityLevel;
            this.sprite = new SpriteObject(texture, pos, size);
            sprite.layerDepth = 0.0f;
            this.bounds = new Vectangle(pos, size);
        }

        public UIMouseResponder GetMouseHover(Vector2 localMousePos)
        {
            return bounds.Contains(localMousePos) ? this : null;
        }

        public virtual UIMouseResponder GetOverlayMouseHover(Vector2 localMousePos)
        {
            return null;
        }

        public UIMouseResponder GetPipeMouseHover(Vector2 localMousePos)
        {
            foreach(OutputPipe pipe in pipes)
            {
                UIMouseResponder result = pipe.GetMouseHover(localMousePos);
                if (result != null)
                    return result;
            }

            return null;
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

        public virtual void Update(CityUIBlackboard blackboard)
        {
            InputState inputState = blackboard.inputState;
            if (inputState.WasMouseLeftJustPressed() && inputState.hoveringElement == this)
            {
                blackboard.selectedObject = this;
            }

            if (inputState.hoveringElement == this && inputState.mouseLeft.isDown && !dragging && canDrag && inputState.MouseDelta.LengthSquared() > 0)
            {
                dragging = true;
                draggingOffset = inputState.OldMousePos - bounds.Origin;
            }

            if (dragging && blackboard.selectedObject == this)
            {
                if (inputState.mouseLeft.isDown)
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

            UpdateUnlimitedPipes();

            selected = (blackboard.selectedObject == this);
        }

        public void UpdateUnlimitedPipes()
        {
            if (!unlimitedPipes)
                return;

            bool foundOneDisconnected = false;
            int Idx = 0;
            while (Idx < pipes.Count)
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

        public virtual void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            sprite.Draw(spriteBatch);
        }

        public virtual void DrawDraggingUI(SpriteBatch spriteBatch, CityUIBlackboard blackboard) { }

        public void DisconnectPipes()
        {
            foreach(OutputPipe pipe in pipes)
            {
                pipe.ConnectTo(null);
            }

            if(pipeSocket != null)
            {
                while(pipeSocket.connectedPipes.Count > 0)
                {
                    pipeSocket.connectedPipes.First().ConnectTo(null);
                }
            }
        }

        public static CityObject FromTemplate(CityLevel cityLevel, JSONTable template)
        {
            string type = template.getString("type");
            switch (type)
            {
                case "inbox":
                    return new ChemicalInbox(cityLevel, template);
                case "outbox":
                    return new ChemicalOutbox(cityLevel, template);
                case "factory":
                    return new ChemicalFactory(cityLevel, template);
                case "crystalOutbox":
                    return new CrystalOutbox(cityLevel, template);
                default:
                    throw new ArgumentException("Unknown CityObject type \"" + type + "\"");
            }
        }
    }
}
