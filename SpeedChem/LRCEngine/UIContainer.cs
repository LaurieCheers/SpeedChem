using LRCEngine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRCEngine
{
    public interface UIElement
    {
        void Update(InputState inputState);
        void Draw(SpriteBatch spriteBatch);
    }

    public class UIContainer :UIElement
    {
        List<UIElement> elements = new List<UIElement>();

        public void Update(InputState inputState)
        {
            foreach (UIElement element in elements)
                element.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (UIElement element in elements)
                element.Draw(spriteBatch);
        }

        public void Add(UIElement element)
        {
            elements.Add(element);
        }
    }
}
