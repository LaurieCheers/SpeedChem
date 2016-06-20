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
        public static Texture2D block;
        public static Texture2D white;
        public static Texture2D clear;
        public static Texture2D character;
        public static Texture2D factory;
        public static Texture2D grassy_factory;
        public static Texture2D silo;
        public static Texture2D grassy_silo;
        public static Texture2D building_site;
        public static Texture2D chemIcon;
        public static Texture2D pipe;
        public static Texture2D grassy_pipe;
        public static Texture2D pipeHandle;
        public static Texture2D inbox;
        public static Texture2D outbox;
        public static Texture2D warning;
        public static Texture2D bubbleBlock;
        public static Texture2D steelBlock;
        public static Texture2D glassBlock;
        public static Texture2D wildcardBlock;
        public static Texture2D bubbleIcon;
        public static Texture2D glassIcon;
        public static Texture2D wildcardIcon;
        public static Texture2D cement;
        public static Texture2D woodFloor;
        public static Texture2D buttonHood;
        public static Texture2D cuttingBeam;
        public static Texture2D depot;
        public static Texture2D hourglass;
        public static Texture2D hourglass_frozen;
        public static Texture2D drag_prompt;
        public static Texture2D city;
        public static Texture2D mapBG;
        public static Texture2D plinth;
        public static Texture2D core_fill;

        public static Texture2D processor;
        public static Texture2D[] spools;
        public static Texture2D[] cores;

        public static Texture2D rivetgun;
        public static Texture2D cutting_laser;
        public static Texture2D bubblegun;
        public static Texture2D jetpack;
        public static Texture2D empty_core;

        public static Texture2D lmb;
        public static Texture2D rmb;

        public static RichImage grass;
        public static RichImage levelbg;
        public static RichImage castIronButton;
        public static RichImage castIronButton_hover;
        public static RichImage castIronButton_pressed;
        public static RichImage steelButton;
        public static RichImage steelButton_hover;
        public static RichImage steelButton_pressed;
        public static RichImage outlined_square;
        public static RichImage cores_bar;
        public static RichImage bad_cores_bar;
        public static RichImage keyboard_key;
        public static RichImage screw_panel;

        public static void Load(ContentManager Content)
        {
            block = Content.Load<Texture2D>("button3d");
            white = Content.Load<Texture2D>("white");
            character = Content.Load<Texture2D>("brunel");
            clear = Content.Load<Texture2D>("clear");
            factory = Content.Load<Texture2D>("factory");
            grassy_factory = Content.Load<Texture2D>("grassy_factory");
            silo = Content.Load<Texture2D>("silo");
            plinth = Content.Load<Texture2D>("plinth");
            grassy_silo = Content.Load<Texture2D>("grassy_silo");
            building_site = Content.Load<Texture2D>("building_site");
            chemIcon = Content.Load<Texture2D>("chemicon");
            pipe = Content.Load<Texture2D>("pipe");
            grassy_pipe = Content.Load<Texture2D>("grassy_pipe");
            pipeHandle = Content.Load<Texture2D>("pipehandle");
            inbox = Content.Load<Texture2D>("mine");
            outbox = Content.Load<Texture2D>("hq");
            warning = Content.Load<Texture2D>("warning");
            bubbleBlock = Content.Load<Texture2D>("bubble");
            glassBlock = Content.Load<Texture2D>("glassblock");
            wildcardBlock = Content.Load<Texture2D>("wildcard");
            steelBlock = Content.Load<Texture2D>("steel");
            bubbleIcon = Content.Load<Texture2D>("bubbleicon");
            glassIcon = Content.Load<Texture2D>("glassicon");
            wildcardIcon = Content.Load<Texture2D>("wildcard_icon");
            cement = Content.Load<Texture2D>("cement");
            woodFloor = Content.Load<Texture2D>("woodfloor");
            buttonHood = Content.Load<Texture2D>("buttonhood");
            cuttingBeam = Content.Load<Texture2D>("cuttingbeam");
            bubblegun = Content.Load<Texture2D>("bubblegun");
            jetpack = Content.Load<Texture2D>("jetpack");
            depot = Content.Load<Texture2D>("depot");
            hourglass = Content.Load<Texture2D>("hourglass");
            hourglass_frozen = Content.Load<Texture2D>("hourglass_frozen");
            rivetgun = Content.Load<Texture2D>("rivetgun");
            cutting_laser = Content.Load<Texture2D>("cutting_laser");
            empty_core = Content.Load<Texture2D>("empty_core");
            drag_prompt = Content.Load<Texture2D>("drag_prompt");
            city = Content.Load<Texture2D>("city");
            mapBG = Content.Load<Texture2D>("England");
            processor = Content.Load<Texture2D>("processor");
            spools = new Texture2D[]
            {
                Content.Load<Texture2D>("spool1"),
                Content.Load<Texture2D>("spool2"),
                Content.Load<Texture2D>("spool3"),
                Content.Load<Texture2D>("spool4")
            };
            cores = new Texture2D[]
            {
                Content.Load<Texture2D>("core_anim1"),
                Content.Load<Texture2D>("core_anim2"),
                Content.Load<Texture2D>("core_anim3"),
                Content.Load<Texture2D>("core_anim4"),
            };
            core_fill = Content.Load<Texture2D>("core_fill");
 
            lmb = Content.Load<Texture2D>("lmb");
            rmb = Content.Load<Texture2D>("rmb");

            grass = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("grass"), Color.White, RichImageDrawMode.TILED, 0, Rotation90.None));
            levelbg = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("levelbg"), Color.White, RichImageDrawMode.TILED, 0, Rotation90.None));
            cores_bar = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("cores_bar"), Color.White, RichImageDrawMode.TILEDPROGRESSBAR, 0, Rotation90.None));
            bad_cores_bar = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("bad_cores_bar"), Color.White, RichImageDrawMode.TILEDPROGRESSBAR, 0, Rotation90.None));
            castIronButton = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            castIronButton_hover = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton_hover"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            castIronButton_pressed = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("castIronButton_pressed"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            steelButton = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("steelButton"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            steelButton_hover = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("steelButton_hover"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            steelButton_pressed = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("steelButton_pressed"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            outlined_square = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("outlined_square"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            keyboard_key = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("keyboard_key"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
            screw_panel = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("screw_panel"), Color.White, RichImageDrawMode.STRETCHED9GRID, 0, Rotation90.None));
        }

    }
}
