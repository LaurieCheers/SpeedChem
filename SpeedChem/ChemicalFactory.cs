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
    public class ChemicalFactory: SpriteObject
    {
        UIContainer ui;

        public ChemicalFactory(Texture2D texture, Vector2 pos, Vector2 size): base(texture, pos, size)
        {
            ui = new UIContainer();
            ui.Add(new UIButton("Play", new Rectangle((int)pos.X-19, (int)pos.Y + 70, 70, 35), Game1.buttonStyle, button_Play));
        }

        public void button_Play()
        {
            Game1.instance.ViewFactory(this);
        }

        public void Update(InputState inputState, bool isBackground)
        {
            if(!isBackground)
            {
                ui.Update(inputState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            ui.Draw(spriteBatch);
        }
    }
}
