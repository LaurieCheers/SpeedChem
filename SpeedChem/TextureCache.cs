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
        public Texture2D grassy_factory;
        public Texture2D silo;
        public Texture2D chemIcon;
        public Texture2D pipe;
        public Texture2D grassy_pipe;
        public Texture2D pipeHandle;
        public Texture2D inbox;
        public Texture2D outbox;
        public Texture2D warning;
        public Texture2D bubbleBlock;
        public Texture2D steelBlock;
        public Texture2D glassBlock;
        public Texture2D bubbleIcon;
        public Texture2D glassIcon;
        public Texture2D cement;
        public Texture2D woodFloor;
        public Texture2D buttonHood;
        public Texture2D cuttingBeam;
        public Texture2D depot;
        public Texture2D hourglass;
        public Texture2D hourglass_frozen;
        public Texture2D drag_prompt;
        public Texture2D city;
        public Texture2D mapBG;

        public Texture2D rivetgun;
        public Texture2D cutting_laser;
        public Texture2D empty_core;

        public RichImage grass;
        public RichImage levelbg;
        public RichImage castIronButton;
        public RichImage castIronButton_hover;
        public RichImage castIronButton_pressed;
        public RichImage castIronButton_active;
        public RichImage outlined_square;
        public RichImage cores_bar;
        public RichImage bad_cores_bar;

        public TextureCache(ContentManager Content)
        {
            block = Content.Load<Texture2D>("button3d");
            white = Content.Load<Texture2D>("white");
            character = Content.Load<Texture2D>("brunel");
            clear = Content.Load<Texture2D>("clear");
            factory = Content.Load<Texture2D>("factory");
            grassy_factory = Content.Load<Texture2D>("grassy_factory");
            silo = Content.Load<Texture2D>("silo");
            chemIcon = Content.Load<Texture2D>("chemicon");
            pipe = Content.Load<Texture2D>("pipe");
            grassy_pipe = Content.Load<Texture2D>("grassy_pipe");
            pipeHandle = Content.Load<Texture2D>("pipehandle");
            inbox = Content.Load<Texture2D>("mine");
            outbox = Content.Load<Texture2D>("hq");
            warning = Content.Load<Texture2D>("warning");
            bubbleBlock = Content.Load<Texture2D>("bubble");
            glassBlock = Content.Load<Texture2D>("glassblock");
            steelBlock = Content.Load<Texture2D>("steel");
            bubbleIcon = Content.Load<Texture2D>("bubbleicon");
            glassIcon = Content.Load<Texture2D>("glassicon");
            cement = Content.Load<Texture2D>("cement");
            woodFloor = Content.Load<Texture2D>("woodfloor");
            buttonHood = Content.Load<Texture2D>("buttonhood");
            cuttingBeam = Content.Load<Texture2D>("cuttingbeam");
            depot = Content.Load<Texture2D>("depot");
            hourglass = Content.Load<Texture2D>("hourglass");
            hourglass_frozen = Content.Load<Texture2D>("hourglass_frozen");
            rivetgun = Content.Load<Texture2D>("rivetgun");
            cutting_laser = Content.Load<Texture2D>("cutting_laser");
            empty_core = Content.Load<Texture2D>("empty_core");
            drag_prompt = Content.Load<Texture2D>("drag_prompt");
            city = Content.Load<Texture2D>("city");
            mapBG = Content.Load<Texture2D>("England");

            grass = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("grass"), Color.White, RichImageDrawMode.TILED, 0, Rotation90.None));
            levelbg = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("levelbg"), Color.White, RichImageDrawMode.TILED, 0, Rotation90.None));
            cores_bar = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("cores_bar"), Color.White, RichImageDrawMode.TILEDPROGRESSBAR, 0, Rotation90.None));
            bad_cores_bar = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("bad_cores_bar"), Color.White, RichImageDrawMode.TILEDPROGRESSBAR, 0, Rotation90.None));
            castIronButton = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            castIronButton_hover = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton_hover"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            castIronButton_pressed = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton_pressed"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            castIronButton_active = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton_active"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            outlined_square = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("outlined_square"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
        }

    }
}
