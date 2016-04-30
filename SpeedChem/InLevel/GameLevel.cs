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
    class GameLevel
    {
        List<WorldObject> objects;
        List<Projectile> projectiles;
        List<Command> triggerables;
        UIContainer ui;
        public bool active { get; private set; }

        public GameLevel()
        {
            InitObjects();

            ui = new UIContainer();
            ui.Add(new UIButton("Reset", new Rectangle(500,30, 100,50), Game1.buttonStyle, button_Reset));
            ui.Add(new UIButton("Back", new Rectangle(500, 90, 100, 50), Game1.buttonStyle, button_Back));
        }

        void InitObjects()
        {
            objects = new List<WorldObject>();
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(0, 0), new Vector2(32, 192)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(192, 0), new Vector2(160, 32)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(352, 0), new Vector2(32, 320)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(16, 124), new Vector2(32, 32)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(0, 192), new Vector2(224, 32)));
            objects.Add(new WorldObject(Game1.textures.block, new Vector2(192, 224), new Vector2(32, 96)));

            triggerables = new List<Command>();
            Command_Spawn spawner = new Command_Spawn(new Vectangle(92, 32, 78, 36), new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.GREEN }));
            triggerables.Add(spawner);

            objects.Add(new PushButton(spawner, Game1.textures.clear, Game1.textures.white, new Vector2(32, 160), new Vector2(8, 32), Color.Red));
            objects.Add(new OutputZone(Game1.textures.white, new Vector2(192, 300), new Vector2(160, 32)));

            PlatformCharacter player = new PlatformCharacter(Game1.textures.character, new Vector2(50, 100), new Vector2(22, 32), Color.White, new Rectangle(10, 0, 22, 32));
            objects.Add(player);

            projectiles = new List<Projectile>();
            active = true;
        }

        public void button_Reset()
        {
            InitObjects();
        }

        public void button_Back()
        {
            active = false;
        }

        public void Update(InputState inputState)
        {
            if (!active)
                return;

            bool needsDestroy = false;
            foreach (WorldObject obj in objects)
            {
                obj.Update(inputState, objects, projectiles);
                if (obj.destroyed)
                    needsDestroy = true;
            }
            if(needsDestroy)
            {
                for (int oIdx = objects.Count - 1; oIdx >= 0; --oIdx)
                {
                    if (objects[oIdx].destroyed)
                    {
                        objects[oIdx] = objects[objects.Count - 1];
                        objects.RemoveAt(objects.Count - 1);
                    }
                }
            }
            for (int pIdx = projectiles.Count - 1; pIdx >= 0; --pIdx)
            {
                projectiles[pIdx].Update(objects);
                if (projectiles[pIdx].destroyed)
                {
                    projectiles[pIdx] = projectiles[projectiles.Count - 1];
                    projectiles.RemoveAt(projectiles.Count - 1);
                }
            }
            foreach (Command triggerable in triggerables)
            {
                triggerable.Update(objects);
            }

            ui.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!active)
                return;

            foreach (WorldObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }

            ui.Draw(spriteBatch);
        }
    }
}
