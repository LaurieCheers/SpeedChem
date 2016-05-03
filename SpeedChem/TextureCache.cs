using LRCEngine;
using Microsoft.Xna.Framework;
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
        public Texture2D factory;
        public Texture2D silo;
        public Texture2D chemIcon;
        public Texture2D pipe;
        public Texture2D pipeHandle;
        public Texture2D inbox;
        public Texture2D outbox;
        public Texture2D warning;

        public RichImage grass;

        public TextureCache(ContentManager Content)
        {
            block = Content.Load<Texture2D>("button3d");
            white = Content.Load<Texture2D>("white");
            character = Content.Load<Texture2D>("bodyguard");
            clear = Content.Load<Texture2D>("clear");
            factory = Content.Load<Texture2D>("factory");
            silo = Content.Load<Texture2D>("silo");
            chemIcon = Content.Load<Texture2D>("chemicon");
            pipe = Content.Load<Texture2D>("pipe");
            pipeHandle = Content.Load<Texture2D>("pipehandle");
            inbox = Content.Load<Texture2D>("helipad");
            outbox = Content.Load<Texture2D>("hq");
            warning = Content.Load<Texture2D>("warning");

            grass = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("grass"), Color.White, RichImageDrawMode.TILED, 0, Rotation90.None));
        }

    }
}
