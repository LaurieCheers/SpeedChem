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
    public class WorldObject : UIMouseResponder
    {
        SpriteObject sprite;
        public Vectangle bounds;
        public virtual float incomePerSecond { get { return 0; } }

        public WorldObject(Texture2D texture, Vector2 pos, Vector2 size)
        {
            sprite = new SpriteObject(texture, pos, size);
            sprite.layerDepth = 0.0f;
            bounds = new Vectangle(pos, size);
        }

        public WorldObject(Texture2D texture, Vector2 pos)
        {
            sprite = new SpriteObject(texture, pos);
            sprite.layerDepth = 0.0f;
            bounds = new Vectangle(pos, texture.Size());
        }

        public UIMouseResponder GetMouseHover(Vector2 localMousePos)
        {
            return bounds.Contains(localMousePos) ? this : null;
        }

        public virtual void Run()
        {
        }

        public virtual void Update(InputState inputState)
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
