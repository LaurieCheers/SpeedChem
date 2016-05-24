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
        GAINCRYSTAL,
        SPENDCRYSTAL,
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

        public FactoryCommand(int currentTime, FactoryCommandType type)
        {
            this.time = currentTime;
            this.type = type;
        }
    }

    public class ChemicalFactory : CityObject
    {
        class FactoryThread
        {
            public int currentTime;
            public int nextCommandIdx;
            public bool stalled;
        };
        UIContainer ui;
        bool paused = false;
        public ChemicalSignature queuedOutput;
        List<FactoryCommand> commands = new List<FactoryCommand>();
        List<FactoryThread> threads = new List<FactoryThread>();
        public int numCores { get; private set; }
        public readonly ChemicalSignature internalSeller;
        public readonly int sellerPrice;
        public readonly FactoryCommandType sellerAction;
        public const int TIME_PER_CORE = 4;
        public const int DEFAULT_NUM_CORES = 6;
        public const int CORES_BOX_WIDTH = 30;
        public const int CORES_BOX_HEIGHT = 20;
        public const int FRAMES_BETWEEN_OUTPUTS = 10;
        public int framesBeforeNextOutput = 0;

        public ChemicalFactory(CityLevel cityLevel, JSONTable template): base(
            cityLevel,
            template.getBool("movable", true)? Game1.textures.factory : Game1.textures.grassy_factory,
            template.getVector2("pos"),
            Game1.textures.factory.Size()
          )
        {
            numCores = DEFAULT_NUM_CORES;
            ui = new UIContainer(bounds.XY);
            ui.Add(new UIButton("Record", new Rectangle(((int)bounds.Width - 90) / 2, -40, 90, 35), Game1.buttonStyle, button_Play));
            canDrag = template.getBool("movable", true);

            SetPipeSocket(new Vector2(32, 16), 2);
            AddOutputPipe(new Vector2(32, 16));
            unlimitedPipes = canDrag;
        }

        public ChemicalFactory(CityLevel cityLevel, Vector2 pos, bool movable): base(cityLevel, movable? Game1.textures.factory: Game1.textures.grassy_factory, pos, Game1.textures.factory.Size())
        {
            numCores = DEFAULT_NUM_CORES;
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(((int)bounds.Width-90)/2, -40, 90, 35), Game1.buttonStyle, button_Play));

            canDrag = movable;

            SetPipeSocket(new Vector2(32, 16), 2);
            AddOutputPipe(new Vector2(32, 16));
            unlimitedPipes = true;
        }

        public ChemicalFactory(CityLevel cityLevel, ChemicalSignature internalSeller, int sellerPrice, Vector2 pos) : base(cityLevel, Game1.textures.outbox, pos, Game1.textures.outbox.Size())
        {
            numCores = DEFAULT_NUM_CORES;
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(-20, (int)bounds.Height + 54, 90, 35), Game1.buttonStyle, button_Play));

            this.internalSeller = internalSeller;
            this.sellerPrice = sellerPrice;
            this.sellerAction = FactoryCommandType.EARNMONEY;
            this.canDrag = false;

            SetPipeSocket(new Vector2(16, 48), 2);
        }

        public ChemicalFactory(CityLevel cityLevel, ChemicalSignature internalSeller, FactoryCommandType sellerAction, Vector2 pos) : base(cityLevel, Game1.textures.depot, pos, Game1.textures.depot.Size())
        {
            numCores = DEFAULT_NUM_CORES;
            ui = new UIContainer(pos);
            ui.Add(new UIButton("Record", new Rectangle(-20, (int)bounds.Height+54, 90, 35), Game1.buttonStyle, button_Play));

            this.canDrag = false;
            this.internalSeller = internalSeller;
            this.sellerPrice = 1;
            this.sellerAction = sellerAction;

            SetPipeSocket(new Vector2(16, 16), 2);
        }

        public override UIMouseResponder GetOverlayMouseHover(Vector2 localMousePos)
        {
            if (selected)
            {
                return ui.GetMouseHover(localMousePos);
            }
            return null;
        }

        public void button_Play()
        {
            Game1.instance.ViewFactory(this);
        }

        public bool PushOutput(ChemicalSignature signature)
        {
            foreach (OutputPipe pipe in pipes)
            {
                PipeSocket socket = pipe.connectedTo;
                if (socket != null)
                {
                    if (queuedOutput != null)
                    {
                        if (socket.parent.ReceiveInput(queuedOutput))
                        {
                            pipe.AnimatePip();
                            didOutput = true;
                            queuedOutput = null;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (socket.parent.ReceiveInput(signature))
                    {
                        pipe.AnimatePip();
                        didOutput = true;
                        return true;
                    }
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
                didOutput = true;
                pipe.AnimatePip();
                return result;
            }

            return null;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            if (queuedOutput != null)
                return queuedOutput;

            // anticipate what kind of chemicals we output next
            if (commands.Count > 0)
            {
                for (int Idx = 0; Idx < commands.Count; ++Idx)
                {
                    if (commands[Idx].type == FactoryCommandType.OUTPUT)
                        return commands[Idx].signature;
                }
                /*                for (int Idx = nextCommandIdx; Idx < commands.Count; ++Idx)
                                {
                                    if (commands[Idx].type == FactoryCommandType.OUTPUT)
                                        return commands[Idx].signature;
                                }

                                for (int Idx = 0; Idx < nextCommandIdx; ++Idx)
                                {
                                    if (commands[Idx].type == FactoryCommandType.OUTPUT)
                                        return commands[Idx].signature;
                                }*/
            }

            return null;
        }

        public void SaveRecording(List<FactoryCommand> recordedCommands)
        {
            commands = recordedCommands.ToList();
            UpdateThreads();
        }

        void UpdateThreads()
        {
            int numThreads;
            if (commands.Count == 0)
            {
                numThreads = 0;
            }
            else
            {
                int coresPerRecording = (int)Math.Ceiling(commands.Last().time / (60.0f * TIME_PER_CORE));
                numThreads = (int)Math.Floor(numCores / (float)coresPerRecording);
                if (numThreads == 0)
                    numThreads = 1;
            }

            threads = new List<FactoryThread>(numThreads);
            for (int Idx = 0; Idx < numThreads; ++Idx)
                threads.Add(new FactoryThread());
            queuedOutput = null;
        }

        public override void Run()
        {
            bool allFinished = true;
            if (framesBeforeNextOutput > 0)
                framesBeforeNextOutput--;

            foreach (FactoryThread thread in threads)
            {
                int oldTime = thread.currentTime;
                if (thread.nextCommandIdx != -1 && commands.Count > thread.nextCommandIdx)
                {
                    FactoryCommand command = commands[thread.nextCommandIdx];
                    if (command.time == thread.currentTime)
                    {
                        switch (command.type)
                        {
                            case FactoryCommandType.INPUT:
                                if (ConsumeInput(command.signature) != null)
                                {
                                    thread.currentTime++;
                                    thread.nextCommandIdx++;
                                }
                                break;
                            case FactoryCommandType.OUTPUT:
                                if (framesBeforeNextOutput == 0 && PushOutput(command.signature))
                                {
                                    framesBeforeNextOutput = FRAMES_BETWEEN_OUTPUTS;
                                    thread.currentTime++;
                                    thread.nextCommandIdx++;
                                }
                                break;
                            case FactoryCommandType.EARNMONEY:
                                Game1.instance.inventory.GainMoney(command.amount, this.bounds.Center, cityLevel);
                                didOutput = true;
                                thread.currentTime++;
                                thread.nextCommandIdx++;
                                break;
                            case FactoryCommandType.GAINCRYSTAL:
                                Game1.instance.inventory.GainCrystals(1);
                                thread.currentTime++;
                                thread.nextCommandIdx++;
                                break;
                            case FactoryCommandType.SPENDCRYSTAL:
                                if (Game1.instance.inventory.SpendCrystals(1))
                                {
                                    thread.currentTime++;
                                    thread.nextCommandIdx++;
                                }
                                break;
                        }
                    }
                    else
                    {
                        thread.currentTime++;
                    }

                    allFinished = false;
                    thread.stalled = (thread.currentTime == oldTime && commands.Count > 0);
                }
                else
                {
                    thread.nextCommandIdx = 0;
                    thread.currentTime = 0;
                }
            }

/*            if (allFinished)
            {
                foreach (FactoryThread thread in threads)
                {
                    thread.nextCommandIdx = 0;
                    thread.currentTime = 0;
                }
            }
            */
        }

        public void AddCores(int numExtraCores)
        {
            numCores += numExtraCores;
            UpdateThreads();
        }

        public override void Update(CityUIBlackboard blackboard)
        {
            Rectangle dragBoxRect = GetDragBox();
            if (selected && numCores > DEFAULT_NUM_CORES && dragBoxRect.Contains(blackboard.inputState.MousePos) && blackboard.inputState.WasMouseLeftJustPressed())
            {
                // unmerging a previously merged factory
                numCores -= DEFAULT_NUM_CORES;
                UpdateThreads();
                ChemicalFactory newFactory = new ChemicalFactory(cityLevel, blackboard.inputState.MousePos, true);
                blackboard.cityLevel.AddObjectDeferred(newFactory);
                blackboard.selectedObject = newFactory;
                return;
            }

            base.Update(blackboard);

            if (selected)
            {
                ui.origin = bounds.Origin;
                ui.Update(blackboard.inputState);
            }

            if (blackboard.selectedObject != null
                && blackboard.selectedObject != this
                && blackboard.selectedObject is ChemicalFactory
                && blackboard.selectedObject.dragging
                && GetDragBox().Contains(blackboard.inputState.MousePos))
            {
                blackboard.draggingOntoObject = this;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            if (blackboard.selectedObject == this && blackboard.draggingOntoObject != null)
            {
                return;
            }

            base.Draw(spriteBatch, blackboard);

            foreach(FactoryThread thread in threads)
            {
                if (thread.stalled)
                {
                    spriteBatch.Draw(Game1.textures.warning, new Rectangle((int)bounds.X, (int)bounds.Y, 16, 16), Color.White);
                    break;
                }
            }

            if(internalSeller != null)
            {
                Vector2 pos = new Vector2(bounds.X, bounds.Y + bounds.Height);
                Vector2 signatureSize = new Vector2(internalSeller.width * 8, internalSeller.height * 8);

                string text = "";
                switch(sellerAction)
                {
                    case FactoryCommandType.EARNMONEY:
                        text = "$" + sellerPrice;
                        break;
                    case FactoryCommandType.GAINCRYSTAL:
                        text = "crystal";
                        break;
                }
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

                spriteBatch.DrawString(Game1.font, text, textPos, Color.Yellow);
            }

            if (selected)
            {
                ui.Draw(spriteBatch);

                Rectangle dragBoxRect = GetDragBox();

                const int CORE_SIZE = 8;
                const int MAX_CORES_PER_LINE = 5;
                int numCoresDrawn = 0;
                Vector2 initialBarPos = new Vector2((int)bounds.CenterX + 2 - CORE_SIZE*3, (int)(bounds.Bottom + 2));
                Vector2 progressBarPos = initialBarPos;

                if (commands.Count > 0)
                {
                    int recordingDurationFrames = commands.Last().time;
                    float recordingDurationSeconds = recordingDurationFrames / 60.0f;

                    int numCoresDrawnThisLine = 0;
                    int numCoresPerThread = (int)Math.Ceiling(recordingDurationSeconds / TIME_PER_CORE);
                    int progressBarInternalWidth = (numCoresPerThread * CORE_SIZE);
                    foreach (FactoryThread thread in threads)
                    {
                        float maxTimeFraction = 1.0f;//recordingDurationSeconds / (numCoresPerThread * TIME_PER_CORE);
                        spriteBatch.Draw(Game1.textures.white, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, numCoresPerThread * CORE_SIZE, CORE_SIZE), Color.Black);
                        spriteBatch.Draw(Game1.textures.white, new Rectangle((int)progressBarPos.X, (int)(progressBarPos.Y), (int)(progressBarInternalWidth * maxTimeFraction), CORE_SIZE), new Color(100,100,100));
                        float progressFraction = thread.currentTime / (recordingDurationSeconds * 60.0f); // (60.0f* numCoresPerThread * TIME_PER_CORE);
                        spriteBatch.Draw(Game1.textures.white, new Rectangle((int)progressBarPos.X, (int)(progressBarPos.Y), (int)(progressBarInternalWidth * progressFraction), CORE_SIZE), thread.stalled ? Color.Red : new Color(100,200,0));// new Color(120,170,255));
                        spriteBatch.Draw(thread.stalled? Game1.textures.bad_cores_bar: Game1.textures.cores_bar, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, numCoresPerThread * CORE_SIZE, CORE_SIZE), Color.White);

                        numCoresDrawn += numCoresPerThread;
                        numCoresDrawnThisLine += numCoresPerThread;
                        if (numCoresDrawnThisLine <= MAX_CORES_PER_LINE)
                        {
                            progressBarPos.X += numCoresPerThread * CORE_SIZE;
                        }
                        else
                        {
                            progressBarPos.X = initialBarPos.X;
                            progressBarPos.Y += CORE_SIZE + 2;
                            numCoresDrawnThisLine = 0;
                        }
                    }

                    string durationStr = "("+PlatformLevel.TimeToString(recordingDurationFrames)+" secs)";
                    Vector2 textPos = new Vector2((int)bounds.CenterX, (int)(progressBarPos.Y + (numCoresDrawnThisLine == 0? 0: CORE_SIZE) + 2));
                    spriteBatch.DrawString(Game1.font, durationStr, textPos, TextAlignment.CENTER, Color.White);
                }

                while (numCores > numCoresDrawn)
                {
                    spriteBatch.Draw(Game1.textures.empty_core, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, CORE_SIZE, CORE_SIZE), Color.White);
                    progressBarPos.X += CORE_SIZE;
                    numCoresDrawn++;
                }

/*                string text = "" + numCores;
                Vector2 textSize = Game1.font.MeasureString(text);
                spriteBatch.Draw(Game1.textures.outlined_square, dragBoxRect, numCores > DEFAULT_NUM_CORES? Color.Orange: Color.Gray);
                spriteBatch.DrawString(Game1.font, text, new Vector2((int)(dragBoxRect.Center.X - textSize.X / 2), (int)(dragBoxRect.Center.Y - textSize.Y / 2)), Color.Black);*/
            }
        }

        Rectangle GetDragBox()
        {
            if(internalSeller != null)
                return new Rectangle((int)bounds.Left- CORES_BOX_WIDTH/2, (int)bounds.Bottom + CORES_BOX_HEIGHT, CORES_BOX_WIDTH, CORES_BOX_HEIGHT);
            else
                return new Rectangle((int)bounds.Left - CORES_BOX_WIDTH / 2, (int)bounds.Bottom + 4, CORES_BOX_WIDTH, CORES_BOX_HEIGHT);
        }

        public override void DrawDraggingUI(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            if(blackboard.selectedObject != this && blackboard.selectedObject is ChemicalFactory)
            {
                Rectangle dragBoxRect = GetDragBox();

                ChemicalFactory otherFactory = (blackboard.selectedObject as ChemicalFactory);
                spriteBatch.Draw(Game1.textures.outlined_square, dragBoxRect, blackboard.draggingOntoObject==this? Color.Orange : Color.Beige);

                string text = "+" + otherFactory.numCores;
                Vector2 textSize = Game1.font.MeasureString(text);
                spriteBatch.DrawString(Game1.font, text, new Vector2((int)(dragBoxRect.Center.X - textSize.X/2), (int)(dragBoxRect.Center.Y - textSize.Y/2)), Color.Black);

                spriteBatch.Draw(Game1.textures.drag_prompt, new Vector2(dragBoxRect.X-4, dragBoxRect.Bottom-8), Color.White);
            }
        }
    }
}
