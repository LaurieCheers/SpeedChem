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

        public FactoryCommand(int currentTime, FactoryCommandType type, int amount, ChemicalSignature signature)
        {
            this.time = currentTime;
            this.type = type;
            this.amount = amount;
            this.signature = signature;
        }

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
            public int actualElapsedFrames;
            public int internalTime;
            public int nextCommandIdx;
            public bool stalled;
            public int stalledForFrames;

            Sampler.Float durationSamples = new Sampler.Float(4);

            public void ClearDuration()
            {
                durationSamples.Clear();
            }

            public void AddDurationSample()
            {
                durationSamples.AddSample(Game1.FramesToSeconds(actualElapsedFrames));
            }
            public float averageDurationSecs { get
            {
                return durationSamples.average;
            } }
        }
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
        public const int FRAMES_BETWEEN_OUTPUTS = 9;
        public int framesBeforeNextOutput = 0;
        string warningTriangleMessage = "Test Error";
        public bool showErrorMessage;
        int incomePerLoop = 0;
        public float incomePerSecond = 0;
        public PipeSocket leftSocket;
        public PipeSocket rightSocket;

        int spoolAnimIndex = 0;
        int spoolAnimCountdown = 0;

        public ChemicalFactory(CityLevel cityLevel, JSONTable template): base(
            cityLevel,
            TextureCache.processor,//template.getBool("movable", true)? TextureCache.factory : TextureCache.grassy_factory,
            template.getVector2("pos")
          )
        {
            canDrag = template.getBool("movable", true);
            InitPipes();
        }

        public ChemicalFactory(CityLevel cityLevel, Vector2 pos, bool movable): base(cityLevel, TextureCache.processor/*movable? TextureCache.factory: TextureCache.grassy_factory*/, pos)
        {
            canDrag = movable;
            InitPipes();
        }

        public ChemicalFactory(CityLevel cityLevel, ChemicalSignature internalSeller, int sellerPrice, Vector2 pos) : base(cityLevel, TextureCache.outbox, pos, TextureCache.outbox.Size())
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

        public ChemicalFactory(CityLevel cityLevel, ChemicalSignature internalSeller, FactoryCommandType sellerAction, Vector2 pos) : base(cityLevel, TextureCache.depot, pos, TextureCache.depot.Size())
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

        void InitPipes()
        {
            numCores = DEFAULT_NUM_CORES;
            ui = new UIContainer(bounds.XY);
            ui.Add(new UIButton("Record", new Rectangle(((int)bounds.Width - 90) / 2, -40, 90, 35), Game1.buttonStyle, button_Play));

            leftSocket = new PipeSocket(this, new Vector2(16, 16), 1);
            rightSocket = new PipeSocket(this, new Vector2(32, 16), 1);
            AddOutputPipe(new Vector2(24, 38));
            unlimitedPipes = canDrag;
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

        public override PipeSocket GetNearestSocket(Vector2 mousePos)
        {
            if (leftSocket.connectedPipes.Count > 0 || mousePos.X > bounds.CenterX + 5)
                return rightSocket;
            else
                return leftSocket;
        }

        public bool PushOutput(ChemicalSignature signature, ref string errorMessage)
        {
            foreach (OutputPipe pipe in pipes)
            {
                PipeSocket socket = pipe.connectedTo;
                if (socket != null)
                {
                    if (queuedOutput != null)
                    {
                        if (socket.parent.ReceiveInput(queuedOutput, ref errorMessage))
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

                    if (socket.parent.ReceiveInput(signature, ref errorMessage))
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
                errorMessage = "Output not connected";
                return false;
            }

            queuedOutput = signature;
            return true;
        }

        public override bool ReceiveInput(ChemicalSignature signature, ref string errorMessage)
        {
            errorMessage = "Waiting for output";
            return false;
        }

        public ChemicalSignature GetInputChemical(int inputIndex)
        {
            PipeSocket socket = inputIndex == 0 ? leftSocket : rightSocket;
            foreach (OutputPipe pipe in socket.connectedPipes)
            {
                if (pipe == null || pipe.source == null)
                    continue;

                ChemicalSignature signature = pipe.source.GetOutputChemical();
                if (signature == null)
                    continue;

                return signature;
            }

            return null;
        }

        public ChemicalSignature ConsumeInput(int inputIndex, ChemicalSignature specificInput, ref string errorMessage)
        {
            OutputPipe pipe = null;
            PipeSocket socket = inputIndex == 0 ? leftSocket : rightSocket;

            foreach (OutputPipe currentPipe in socket.connectedPipes)
            {
                if (currentPipe.source.GetOutputChemical() == specificInput)
                {
                    pipe = currentPipe;
                    break;
                }
            }

            if (pipe == null)
            {
                errorMessage = "Input not connected";
                return null;
            }

            ChemicalSignature signature = pipe.source.RequestOutput(pipe, ref errorMessage);

            return signature;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe, ref string errorMessage)
        {
            if (queuedOutput != null)
            {
                ChemicalSignature result = queuedOutput;
                queuedOutput = null;
                didOutput = true;
                pipe.AnimatePip();
                return result;
            }

            errorMessage = "Waiting for input";
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

            CalcIncomePerLoop();
        }

        public override void UpdatePipes()
        {
            base.UpdatePipes();
            CalcIncomePerLoop();
        }

        void CalcIncomePerLoop()
        {
            incomePerLoop = 0;
            foreach(FactoryCommand command in commands)
            {
                // for each command, iterate through all inputs and outputs, determine money spent/received
                // add up 
                switch(command.type)
                {
                    case FactoryCommandType.EARNMONEY:
                        incomePerLoop += command.amount;
                        break;

                    case FactoryCommandType.INPUT:
                        {
                            PipeSocket socket = command.amount == 0 ? leftSocket : rightSocket;
                            foreach (OutputPipe pipe in socket.connectedPipes)
                            {
                                if (pipe.source.GetOutputChemical() == command.signature)
                                {
                                    incomePerLoop -= pipe.source.outputPrice;
                                    break;
                                }
                            }
                        }
                        break;

                    case FactoryCommandType.OUTPUT:
                        foreach (OutputPipe pipe in pipes)
                        {
                            if (pipe.connectedTo != null && pipe.connectedTo.parent.GetInputChemical() == command.signature)
                            {
                                incomePerLoop += pipe.connectedTo.parent.inputPrice;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        public override void Run()
        {
            bool allFinished = true;
            if (framesBeforeNextOutput > 0)
                framesBeforeNextOutput--;

            bool newSample = false;

            foreach (FactoryThread thread in threads)
            {
                thread.actualElapsedFrames++;
                int oldTime = thread.internalTime;
                if (thread.nextCommandIdx != -1 && commands.Count > thread.nextCommandIdx)
                {
                    FactoryCommand command = commands[thread.nextCommandIdx];
                    if (command.time <= thread.internalTime)
                    {
                        switch (command.type)
                        {
                            case FactoryCommandType.INPUT:
                                if (ConsumeInput(command.amount, command.signature, ref warningTriangleMessage) != null)
                                {
                                    thread.internalTime++;
                                    thread.nextCommandIdx++;
                                }
                                break;
                            case FactoryCommandType.OUTPUT:
                                if (framesBeforeNextOutput > 0)
                                {
                                    warningTriangleMessage = "";
                                }
                                else if (framesBeforeNextOutput == 0 && PushOutput(command.signature, ref warningTriangleMessage))
                                {
                                    framesBeforeNextOutput = FRAMES_BETWEEN_OUTPUTS;
                                    thread.internalTime++;
                                    thread.nextCommandIdx++;
                                }
                                break;
                            case FactoryCommandType.EARNMONEY:
                                Game1.instance.inventory.GainMoney(command.amount, this.bounds.Center, cityLevel);
                                didOutput = true;
                                thread.internalTime++;
                                thread.nextCommandIdx++;
                                break;
                            case FactoryCommandType.GAINCRYSTAL:
                                Game1.instance.inventory.GainCrystals(1);
                                thread.internalTime++;
                                thread.nextCommandIdx++;
                                break;
                            case FactoryCommandType.SPENDCRYSTAL:
                                if (Game1.instance.inventory.SpendCrystals(1, this.bounds.Center, cityLevel))
                                {
                                    thread.internalTime++;
                                    thread.nextCommandIdx++;
                                }
                                else
                                {
                                    warningTriangleMessage = "Out of bubbles";
                                }
                                break;
                        }
                    }
                    else
                    {
                        thread.internalTime++;
                    }

                    allFinished = false;
                    thread.stalled = (thread.internalTime == oldTime && commands.Count > 0);
                    if (thread.stalled)
                        thread.stalledForFrames++;
                    else
                        thread.stalledForFrames = 0;
                }
                else
                {
                    thread.AddDurationSample();
                    newSample = true;
                    thread.nextCommandIdx = 0;
                    thread.internalTime = 0;
                    thread.actualElapsedFrames = 0;
                }
            }

            CalcIncomePerSecond();

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

        public void CalcIncomePerSecond()
        {
            incomePerSecond = 0;
            foreach (FactoryThread thread in threads)
            {
                if (thread.averageDurationSecs > 0 && (!thread.stalled || Game1.FramesToSeconds(thread.stalledForFrames) < thread.averageDurationSecs))
                {
                    incomePerSecond += incomePerLoop / thread.averageDurationSecs;
                }
            }
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

            showErrorMessage = false;

            if (selected)
            {
                ui.origin = bounds.Origin;
                ui.Update(blackboard.inputState);
            }

            if (blackboard.inputState.hoveringElement == this &&
                GetWarningRect().Contains(blackboard.inputState.MousePos))
            {
                showErrorMessage = true;
            }

            /*
            if (blackboard.selectedObject != null
                && blackboard.selectedObject != this
                && blackboard.selectedObject is ChemicalFactory
                && blackboard.selectedObject.dragging
                && GetDragBox().Contains(blackboard.inputState.MousePos))
            {
                blackboard.draggingOntoObject = this;
            }*/
        }

        public Vectangle GetWarningRect()
        {
            return new Vectangle(bounds.X, bounds.Y, 16, 16);
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            if (blackboard.selectedObject == this && blackboard.draggingOntoObject != null)
            {
                return;
            }

            base.Draw(spriteBatch, blackboard);

            bool anyThreadRunning = false;
            foreach (FactoryThread thread in threads)
            {
                if (thread.stalled && warningTriangleMessage != "")
                {
                    Vectangle warningRect = GetWarningRect();
                    spriteBatch.Draw(TextureCache.warning, warningRect, Color.White);

                    if(showErrorMessage)
                    {
                        Vector2 messageSize = Game1.font.MeasureString(warningTriangleMessage);
                        if (messageSize.X > warningRect.Left)
                        {
                            spriteBatch.Draw(TextureCache.white, Game1.font.GetStringBounds(warningTriangleMessage, warningRect.TopRight, TextAlignment.LEFT).Bloat(2), new Color(0.25f,0,0,0.5f));
                            spriteBatch.DrawString(Game1.font, warningTriangleMessage, warningRect.TopRight, Color.Yellow);
                        }
                        else
                        {
                            spriteBatch.Draw(TextureCache.white, Game1.font.GetStringBounds(warningTriangleMessage, warningRect.TopLeft, TextAlignment.RIGHT).Bloat(2), new Color(0.25f, 0, 0, 0.5f));
                            spriteBatch.DrawString(Game1.font, warningTriangleMessage, warningRect.TopLeft, TextAlignment.RIGHT, Color.Yellow);
                        }
                    }
                    break;
                }
                else
                {
                    anyThreadRunning = true;
                }
            }

            if (anyThreadRunning)
            {
                spoolAnimCountdown--;
                if (spoolAnimCountdown <= 0)
                {
                    spoolAnimIndex = (spoolAnimIndex+1)%TextureCache.spools.Length;
                    spoolAnimCountdown = 3;
                }
            }
            spriteBatch.Draw(TextureCache.spools[spoolAnimIndex], bounds.XY + new Vector2(7, 32), Color.White);
            spriteBatch.Draw(TextureCache.spools[spoolAnimIndex], bounds.XY + new Vector2(24, 32), Color.White);

            if (internalSeller != null)
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
                    float recordingDurationSeconds = Game1.FramesToSeconds(recordingDurationFrames);

                    int numCoresDrawnThisLine = 0;
                    int numCoresPerThread = (int)Math.Ceiling(recordingDurationSeconds / TIME_PER_CORE);
                    int progressBarInternalWidth = (numCoresPerThread * CORE_SIZE);
                    foreach (FactoryThread thread in threads)
                    {
                        float maxTimeFraction = 1.0f;//recordingDurationSeconds / (numCoresPerThread * TIME_PER_CORE);
                        spriteBatch.Draw(TextureCache.white, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, numCoresPerThread * CORE_SIZE, CORE_SIZE), Color.Black);
                        spriteBatch.Draw(TextureCache.white, new Rectangle((int)progressBarPos.X, (int)(progressBarPos.Y), (int)(progressBarInternalWidth * maxTimeFraction), CORE_SIZE), new Color(100,100,100));
                        float progressFraction = thread.internalTime / (recordingDurationSeconds * 60.0f); // (60.0f* numCoresPerThread * TIME_PER_CORE);
                        spriteBatch.Draw(TextureCache.white, new Rectangle((int)progressBarPos.X, (int)(progressBarPos.Y), (int)(progressBarInternalWidth * progressFraction), CORE_SIZE), thread.stalled ? Color.Red : new Color(100,200,0));// new Color(120,170,255));
                        spriteBatch.Draw(thread.stalled? TextureCache.bad_cores_bar: TextureCache.cores_bar, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, numCoresPerThread * CORE_SIZE, CORE_SIZE), Color.White);

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
                    spriteBatch.Draw(TextureCache.empty_core, new Rectangle((int)progressBarPos.X, (int)progressBarPos.Y, CORE_SIZE, CORE_SIZE), Color.White);
                    progressBarPos.X += CORE_SIZE;
                    numCoresDrawn++;
                }

/*                string text = "" + numCores;
                Vector2 textSize = Game1.font.MeasureString(text);
                spriteBatch.Draw(TextureCache.outlined_square, dragBoxRect, numCores > DEFAULT_NUM_CORES? Color.Orange: Color.Gray);
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
            /*
            if(blackboard.selectedObject != this && blackboard.selectedObject is ChemicalFactory)
            {
                Rectangle dragBoxRect = GetDragBox();

                ChemicalFactory otherFactory = (blackboard.selectedObject as ChemicalFactory);
                spriteBatch.Draw(TextureCache.outlined_square, dragBoxRect, blackboard.draggingOntoObject==this? Color.Orange : Color.Beige);

                string text = "+" + otherFactory.numCores;
                Vector2 textSize = Game1.font.MeasureString(text);
                spriteBatch.DrawString(Game1.font, text, new Vector2((int)(dragBoxRect.Center.X - textSize.X/2), (int)(dragBoxRect.Center.Y - textSize.Y/2)), Color.Black);

                spriteBatch.Draw(TextureCache.drag_prompt, new Vector2(dragBoxRect.X-4, dragBoxRect.Bottom-8), Color.White);
            }*/
        }
    }
}
