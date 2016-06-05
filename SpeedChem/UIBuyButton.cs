using LRCEngine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    public class UIBuyButton : UIButton
    {
        string baseLabel;
        public UIBuyButton(string label, int price, Rectangle frame, UIButtonStyle style, OnPressDelegate onPress) : base(label, frame, style, onPress)
        {
            this.baseLabel = label;
            this.price = price;
        }

        int _price;
        public int price
        {
            get { return _price; }
            set
            {
                _price = value;
                label = baseLabel + " ($" + price + ")";
            }
        }

        public override void Update(InputState inputState, Vector2 origin)
        {
            SetEnabled(Game1.instance.inventory.money >= price);
            base.Update(inputState, origin);
        }

        protected override void Pressed()
        {
            if (Game1.instance.inventory.PayMoney(price, frame.Center.ToVector2(), Game1.instance.currentScreen))
            {
                if (onPress != null)
                {
                    onPress();
                }
            }
        }
    }
}