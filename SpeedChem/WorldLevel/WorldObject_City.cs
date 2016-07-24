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
    public class WorldObject_City : WorldObject
    {
        public readonly CityLevel cityLevel;
        bool unlocked = false;
        bool justUnlocked = false;
        public override float incomePerSecond { get { return cityLevel.incomePerSecond; } }

        public WorldObject_City(CityLevel cityLevel) : base(TextureCache.city, cityLevel.pos)
        {
            this.cityLevel = cityLevel;
        }

        public override void Run()
        {
            if (!unlocked)
            {
                if (cityLevel.price <= Game1.instance.inventory.money)
                {
                    unlocked = true;
                    if (cityLevel.price > 0)
                    {
                        justUnlocked = true;
                        Game1.instance.inventory.cityJustUnlocked = true;
                    }
                }
                return;
            }

            cityLevel.Run();
        }

        public override void Update(InputState inputState)
        {
            if (!unlocked)
                return;

            if (inputState.WasMouseLeftJustPressed() && inputState.hoveringElement == this)
            {
                Game1.instance.ViewCity(cityLevel);
            }

            if(justUnlocked)
            {
                Game1.instance.splashes.Add(new Splash("UNLOCKED", TextAlignment.CENTER, Game1.font, Color.Orange, bounds.Center, new Vector2(0, -4), 0.92f, 0, 3));
                justUnlocked = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!unlocked)
                return;

            base.Draw(spriteBatch);
            spriteBatch.DrawString(Game1.font, cityLevel.name, bounds.BottomCenter, TextAlignment.CENTER, Color.White);

            if(cityLevel.isComplete)
            {
                spriteBatch.Draw(TextureCache.check, bounds.BottomRight + new Vector2(-16, -16), Color.White);
            }
            else if(cityLevel.isNew)
            {
                spriteBatch.Draw(TextureCache.new_badge, bounds.BottomRight + new Vector2(-16, -16), Color.White);
            }
        }
    }
}
