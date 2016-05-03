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
        OUTPUT
    };

    public class FactoryCommand
    {
        public readonly int time;
        public readonly FactoryCommandType type;
        public readonly ChemicalSignature signature;

        public FactoryCommand(int currentTime, FactoryCommandType type, ChemicalSignature signature)
        {
            this.time = currentTime;
            this.type = type;
            this.signature = signature;
        }
    }

    public class ChemicalFactory: MetaGameObject
    {
        UIContainer ui;
        bool paused = false;
        public ChemicalSignature queuedOutput;
        List<FactoryCommand> commands = new List<FactoryCommand>();
        bool recording;
        bool stalled;
        int currentTime;
        int nextCommandIdx = 0;

        public ChemicalFactory(Vector2 pos): base(Game1.textures.factory, pos, Game1.textures.factory.Size())
        {
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(64, 0, 90, 35), Game1.buttonStyle, button_Play));

            SetPipeSocket(new Vector2(32, 16), 2);
            AddOutputPipe(new Vector2(32, 16));
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

            if(recording && signature != null)
            {
                commands.Add(new FactoryCommand(currentTime, FactoryCommandType.INPUT, signature));
                nextCommandIdx++;
            }

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
            return queuedOutput;
        }

        public void SaveRecording(List<FactoryCommand> recordedCommands)
        {
            commands = recordedCommands.ToList();
            nextCommandIdx = 0;
            currentTime = 0;
        }

        public override void Run()
        {
            int oldTime = currentTime;
            if (recording)
            {
                currentTime++;
            }
            else if (nextCommandIdx != -1 && commands.Count > nextCommandIdx)
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
