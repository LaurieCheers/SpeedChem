using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRCEngine
{
    public abstract class UIElement
    {
        public abstract void Update(InputState inputState, Vector2 origin);
        public void Update(InputState inputState) { Update(inputState, Vector2.Zero);  }
        public abstract void Draw(SpriteBatch spriteBatch, Vector2 origin);
        public void Draw(SpriteBatch spriteBatch) { Draw(spriteBatch, Vector2.Zero); }
    }

    public class UIContainer :UIElement
    {
        public Vector2 origin;
        List<UIElement> elements = new List<UIElement>();

        public UIContainer()
        {

        }

        public UIContainer(Vector2 origin)
        {
            this.origin = origin;
        }

        public override void Update(InputState inputState, Vector2 origin)
        {
            Vector2 newOrigin = origin + this.origin;
            foreach (UIElement element in elements)
                element.Update(inputState, newOrigin);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            Vector2 newOrigin = origin + this.origin;
            foreach (UIElement element in elements)
                element.Draw(spriteBatch, newOrigin);
        }

        public void Add(UIElement element)
        {
            elements.Add(element);
        }
    }
}
