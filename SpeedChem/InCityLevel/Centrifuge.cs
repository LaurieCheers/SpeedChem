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
    public enum CentrifugeMode
    {
        TurnLeft,
        TurnRight,
        Turn180,
    };

    public class Centrifuge : CityObject
    {
        CentrifugeMode mode;
        UIContainer ui;
        ChemicalSignature inputSignature;
        ChemicalSignature outputSignature;
        int timer = 0;
        enum CentrifugeState
        {
            EMPTY,
            SPINNING,
            WAITING,
        };
        CentrifugeState state = CentrifugeState.EMPTY;
        const int EMIT_TIMER = 50;
        public override float ShelfRestOffset { get { return 4; } }

        public Centrifuge(CityLevel cityLevel, JSONTable template) : base(
            cityLevel,
            TextureCache.centrifuge,
            template.getVector2("pos")
          )
        {
            this.inputSignature = new ChemicalSignature(template.getArray("chemical"));
            canDrag = template.getBool("movable", true);
            Init();
        }

        public Centrifuge(CityLevel cityLevel, Vector2 pos) : base(cityLevel, TextureCache.centrifuge, pos)
        {
            Init();
        }

        void Init()
        {
            ui = new UIContainer();

            mode = CentrifugeMode.TurnRight;
            if (inputSignature != null)
            {
                UpdateOutputSignature();
            }

            UIButtonStyle centrifuge_button = new UIButtonStyle(new UIButtonAppearance(Game1.font, Color.Black, new RichImage(TextureCache.centrifuge_button_highlight), Color.Black, new Vector2(1,0)),
                new UIButtonAppearance(Game1.font, Color.Black, new RichImage(TextureCache.centrifuge_button_highlight), Color.Yellow, new Vector2(1, 0)),
                new UIButtonAppearance(Game1.font, Color.Black, new RichImage(TextureCache.centrifuge_button_highlight), Color.Orange, new Vector2(1, 0)),
                new UIButtonAppearance(Game1.font, Color.Black, new RichImage(TextureCache.centrifuge_button_highlight), Color.Black, new Vector2(1, 0)));

            ui.Add(new UIButton("", TextureCache.turnLeft, new Rectangle(-4, 42, 18, 18), centrifuge_button, button_SelectTurnLeft));
            ui.Add(new UIButton("", TextureCache.turn180, new Rectangle(14, 42, 18, 18), centrifuge_button, button_SelectTurn180));
            ui.Add(new UIButton("", TextureCache.turnRight, new Rectangle(33, 42, 18, 18), centrifuge_button, button_SelectTurnRight));

            SetPipeSocket(new Vector2(24, 13), 1);
            AddOutputPipe(new Vector2(14, 44));
            unlimitedPipes = true;
        }

        void button_SelectTurnLeft()
        {
            mode = CentrifugeMode.TurnLeft;
            state = CentrifugeState.EMPTY;
        }
        void button_SelectTurn180()
        {
            mode = CentrifugeMode.Turn180;
            state = CentrifugeState.EMPTY;
        }
        void button_SelectTurnRight()
        {
            mode = CentrifugeMode.TurnRight;
            state = CentrifugeState.EMPTY;
        }

        void UpdateOutputSignature()
        {
            outputSignature = MakeRotation(mode, inputSignature);
        }

        public static ChemicalSignature MakeRotation(CentrifugeMode mode, ChemicalSignature signature)
        {
            if (signature == null)
                return null;

            switch (mode)
            {
                case CentrifugeMode.TurnRight: return signature.Rotate();
                case CentrifugeMode.Turn180: return signature.Rotate().Rotate();
                case CentrifugeMode.TurnLeft: return signature.Rotate().Rotate().Rotate();
                default: throw new InvalidOperationException();
            }
        }

        public override ChemicalSignature GetInputChemical()
        {
            return inputSignature;
        }

        public override ChemicalSignature GetOutputChemical()
        {
            if (outputSignature != null)
                return outputSignature;

            foreach (OutputPipe pipe in pipeSocket.connectedPipes)
            {
                ChemicalSignature tryChemical = pipe.source.GetOutputChemical();
                if (tryChemical != null)
                {
                    outputSignature = tryChemical;
                    return outputSignature;
                }
            }

            return null;
        }

        public override ChemicalSignature RequestOutput(OutputPipe pipe, ref string errorMessage)
        {
            if (state != CentrifugeState.WAITING)
            {
                errorMessage = "Centrifuge not ready";
                return null;
            }

            pipe.AnimatePip();
            state = CentrifugeState.EMPTY;
            return outputSignature;
        }

        Texture2D GetModeIcon(CentrifugeMode mode)
        {
            switch (mode)
            {
                case CentrifugeMode.TurnLeft: return TextureCache.turnLeft;
                case CentrifugeMode.TurnRight: return TextureCache.turnRight;
                case CentrifugeMode.Turn180: return TextureCache.turn180;
                default: return null;
            }
        }

        public override UIMouseResponder GetOverlayMouseHover(Vector2 localMousePos)
        {
            if (selected)
            {
                return ui.GetMouseHover(localMousePos);
            }
            return null;
        }

        public override void Run()
        {
            switch (state)
            {
                case CentrifugeState.EMPTY:
                    ChemicalSignature newInputSig = PullInput();
                    if (newInputSig != null)
                    {
                        inputSignature = newInputSig;
                        outputSignature = MakeRotation(mode, inputSignature);
                        state = CentrifugeState.SPINNING;
                        timer = 0;
                    }
                    break;
                case CentrifugeState.SPINNING:
                    timer++;
                    if (timer >= EMIT_TIMER)
                    {
                        state = CentrifugeState.WAITING;
                    }
                    break;
                case CentrifugeState.WAITING:
                    if(PushOutput(outputSignature))
                    {
                        state = CentrifugeState.EMPTY;
                    }
                    break;
            }
        }

        ChemicalSignature PullInput()
        {
            foreach (OutputPipe currentPipe in this.pipeSocket.connectedPipes)
            {
                string errorMessage = "";
                return currentPipe.source.RequestOutput(currentPipe, ref errorMessage);
            }
            return null;
        }

        public bool PushOutput(ChemicalSignature signature)
        {
            foreach (OutputPipe pipe in pipes)
            {
                PipeSocket socket = pipe.connectedTo;
                if (socket != null)
                {
                    string errorMessage = "";
                    if (socket.parent.ReceiveInput(signature, ref errorMessage))
                    {
                        pipe.AnimatePip();
                        didOutput = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Update(CityUIBlackboard blackboard)
        {
            base.Update(blackboard);

            if (selected)
            {
                ui.origin = bounds.Origin;
                ui.Update(blackboard.inputState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.Draw(spriteBatch, blackboard);

            bool hovering = (blackboard.inputState != null) ? (blackboard.inputState.hoveringElement == this) : false;
            if (hovering || selected)
            {
                spriteBatch.Draw(
                    TextureCache.centrifuge_highlight,
                    bounds,
                    selected ? new Color(0.7f, 0.7f, 1.0f, 1.0f) : new Color(1.0f, 1.0f, 0.0f, 1.0f)
                );
            }

            spriteBatch.Draw(GetModeIcon(mode), bounds.XY + new Vector2(15, 20), Color.White);

            if (outputSignature != null)
            {
                outputSignature.Draw(spriteBatch, bounds.BottomRight + new Vector2(-14, -8), true);
            }
        }

        public override void DrawUI(SpriteBatch spriteBatch, CityUIBlackboard blackboard)
        {
            base.DrawUI(spriteBatch, blackboard);

            if (selected)
            {
                ui.Draw(spriteBatch);
            }
        }
    }
}
