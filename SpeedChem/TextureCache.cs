using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    public class TextureCache
    {
        public Texture2D block;
        public Texture2D white;
        public Texture2D clear;
        public Texture2D character;
        public Texture2D lab;
        public Texture2D silo;
        public Texture2D chemIcon;

        public TextureCache(ContentManager Content)
        {
            block = Content.Load<Texture2D>("button3d");
            white = Content.Load<Texture2D>("white");
            character = Content.Load<Texture2D>("bodyguard");
            clear = Content.Load<Texture2D>("clear");
            lab = Content.Load<Texture2D>("hq");
            silo = Content.Load<Texture2D>("silo");
            chemIcon = Content.Load<Texture2D>("chemicon");
        }

    }
}
