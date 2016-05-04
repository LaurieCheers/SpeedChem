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
    public enum FactoryState
    {
        PAUSED,
        RUNNING,
        STALLED,
        EDITING,
    };

    public enum FactoryCommandType
    {
        INPUT,
        OUTPUT,
        EARNMONEY,
    };

    public class FactoryCommand
    {
        public readonly int time;
        public readonly FactoryCommandType type;
        public readonly ChemicalSignature signature;
        public readonly int amount;

        public FactoryCommand(int currentTime, FactoryCommandType type, ChemicalSignature signature)
        {
            this.time = currentTime;
            this.type = type;
            this.signature = signature;
        }

        public FactoryCommand(int currentTime, FactoryCommandType type, int amount)
        {
            this.time = currentTime;
            this.type = type;
            this.amount = amount;
        }
    }

    public class ChemicalFactory: MetaGameObject
    {
        UIContainer ui;
        bool paused = false;
        public ChemicalSignature queuedOutput;
        List<FactoryCommand> commands = new List<FactoryCommand>();
        bool stalled;
        int currentTime;
        int nextCommandIdx = 0;
        public readonly ChemicalSignature internalSeller;
        public readonly int sellerPrice;

        public ChemicalFactory(Vector2 pos): base(Game1.textures.factory, pos, Game1.textures.factory.Size())
        {
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(64, 0, 90, 35), Game1.buttonStyle, button_Play));

            SetPipeSocket(new Vector2(32, 16), 2);
            AddOutputPipe(new Vector2(32, 16));
        }

        public ChemicalFactory(ChemicalSignature internalSeller, int sellerPrice, Vector2 pos) : base(Game1.textures.outbox, pos, Game1.textures.outbox.Size())
        {
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(-20, 84, 90, 35), Game1.buttonStyle, button_Play));

            this.internalSeller = internalSeller;
            this.sellerPrice = sellerPrice;
            this.canDrag = false;

            SetPipeSocket(new Vector2(16, 48), 2);
        }

        public void button_Play()
        {
            Game1.instance.ViewFactory(this);
        }

        public bool PushOutput(ChemicalSignature signature)
        {
            PipeSocket socket = pipes.First().connectedTo;
            if (socket != null)
            {
                if (queuedOutput != null)
                {
                    if (socket.parent.ReceiveInput(queuedOutput))
                    {
                        pipes.First().AnimatePip();
                        queuedOutput = null;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (socket.parent.ReceiveInput(signature))
                {
                    pipes.First().AnimatePip();
                    return true;
                }
            }

            // failed to output - try to queue it
            if (queuedOutput != null)
            {
                return false;
            }

            queuedOutput = signature;
            return true;
        }

        public ChemicalSignature GetInputChemical(int inputIndex)
        {
            if (pipeSocket.connectedPipes.Count <= inputIndex)
                return null;

            List<OutputPipe> sortedPipes = pipeSocket.connectedPipes.OrderBy(o => o.sourcePos.X).ToList();

            OutputPipe pipe = sortedPipes[inputIndex];
            if (pipe == null || pipe.source == null)
                return null;

            return pipe.source.GetOutputChemical();
        }

        public ChemicalSignature ConsumeInput(int inputIndex)
        {
            if (pipeSocket.connectedPipes.Count <= inputIndex)
                return null;

            List<OutputPipe> sortedPipes = pipeSocket.connectedPipes.OrderBy(o => o.sourcePos.X).ToList();

            OutputPipe pipe = sortedPipes[inputIndex];
            if (pipe == null || pipe.source == null)
                return null;

            ChemicalSignature signature = pipe.source.RequestOutput(pipe);

            return signature;
        }

        public ChemicalSignature ConsumeInput(ChemicalSignature specificInput)
        {
            OutputPipe pipe = null;
            foreach (OutputPipe currentPipe in pipeSocket.connectedPipes)
            {
                if (currentPipe.source.GetOutputChemical() == specificInput)
                {
                    pipe = currentPipe;
                    break;
                }
            }

            if (pipe == null)
                return null;

            ChemicalSignature signature = pipe.source.RequestOutput(pipe);

            return signature;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe)
        {
            if (queuedOutput != null)
            {
                ChemicalSignature result = queuedOutput;
                queuedOutput = null;
                pipe.AnimatePip();
                return result;
            }

            return null;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            if (queuedOutput != null)
                return queuedOutput;

            // anticipate what kind of chemicals we would output
            foreach(FactoryCommand command in commands)
            {
                if (command.type == FactoryCommandType.OUTPUT)
                    return command.signature;
            }

            return null;
        }

        public void SaveRecording(List<FactoryCommand> recordedCommands)
        {
            commands = recordedCommands.ToList();
            nextCommandIdx = 0;
            currentTime = 0;
            queuedOutput = null;
        }

        public override void Run()
        {
            int oldTime = currentTime;
            if (nextCommandIdx != -1 && commands.Count > nextCommandIdx)
            {
                FactoryCommand command = commands[nextCommandIdx];
                if (command.time == currentTime)
                {
                    switch(command.type)
                    {
                        case FactoryCommandType.INPUT:
                            if (ConsumeInput(command.signature) != null)
                            {
                                currentTime++;
                                nextCommandIdx++;
                            }
                            break;
                        case FactoryCommandType.OUTPUT:
                            if(PushOutput(command.signature))
                            {
                                currentTime++;
                                nextCommandIdx++;
                            }
                            break;
                        case FactoryCommandType.EARNMONEY:
                            Game1.instance.metaGame.GainMoney(command.amount, this.bounds.Center);
                            currentTime++;
                            nextCommandIdx++;
                            break;
                    }
                }
                else
                {
                    currentTime++;
                }
            }
            else
            {
                nextCommandIdx = 0;
                currentTime = 0;
            }

            stalled = (currentTime == oldTime && commands.Count > 0);
        }

        public override void Update(InputState inputState, ref object selectedObject)
        {
            base.Update(inputState, ref selectedObject);
            if (selected)
            {
                ui.origin = bounds.Origin;
                ui.Update(inputState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if(stalled)
            {
                spriteBatch.Draw(Game1.textures.warning, new Rectangle((int)bounds.X, (int)bounds.Y, 16, 16), Color.White);
            }

            if(internalSeller != null)
            {
                base.Draw(spriteBatch);
                Vector2 pos = new Vector2(bounds.X, bounds.Y + bounds.Height);
                Vector2 signatureSize = new Vector2(internalSeller.width * 8, internalSeller.height * 8);

                string text = "$" + sellerPrice;
                Vector2 textSize = Game1.font.MeasureString(text);

                Vector2 signaturePos = new Vector2(
                    pos.X + (bounds.Width - (signatureSize.X + textSize.X)) * 0.5f,
                    pos.Y + (textSize.Y - signatureSize.Y) * 0.5f
                );

                Vector2 textPos = new Vector2(
                    signaturePos.X + signatureSize.X,
                    pos.Y
                );

                internalSeller.Draw(spriteBatch, signaturePos, true);

                spriteBatch.DrawString(Game1.font, "$" + sellerPrice, textPos, Color.Yellow);
            }

            if (selected)
            {
                ui.Draw(spriteBatch);

                if(commands.Count > 0)
                {
                    string currentStr = GameLevel.TimeToString(currentTime);
                    string finalStr = " / "+ GameLevel.TimeToString(commands.Last().time);
                    Vector2 currentSize = Game1.font.MeasureString(currentStr);
                    Vector2 finalSize = Game1.font.MeasureString(currentStr);
                    float totalWidth = currentSize.X + finalSize.X;
                    float lhsX = bounds.X + 64 + 35 - totalWidth / 2;
                    spriteBatch.DrawString(Game1.font, currentStr, new Vector2((int)lhsX, (int)(bounds.Y - 20)), Color.White);
                    spriteBatch.DrawString(Game1.font, finalStr, new Vector2((int)(lhsX+currentSize.X), (int)(bounds.Y - 20)), Color.Yellow);
                }
            }
        }
    }
}
