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
    public class WorldLevel: SpeedChemScreen
    {
        List<WorldObject> objects = new List<WorldObject>();
        public float incomePerSecond = 0;

        public WorldLevel()
        {
            objects.Add(new WorldObject_City(new CityLevel("TUTOPIA", new Vector2(100, 140))));
            objects.Add(new WorldObject_City(new CityLevel("SHEFFIELD", new Vector2(530, 90))));
            objects.Add(new WorldObject_City(new CityLevel("MANCHESTER", new Vector2(430, 70))));
            objects.Add(new WorldObject_City(new CityLevel("LIVERPOOL", new Vector2(300, 70))));
            objects.Add(new WorldObject_City(new CityLevel("BRISTOL", new Vector2(200, 320))));
            objects.Add(new WorldObject_City(new CityLevel("LONDON", new Vector2(700, 370))));
            objects.Add(new WorldObject_City(new CityLevel("BIRMINGHAM", new Vector2(400, 190))));
            objects.Add(new WorldObject_City(new CityLevel("READING", new Vector2(530, 370))));
            objects.Add(new WorldObject_City(new CityLevel("OXFORD", new Vector2(480, 300))));
            objects.Add(new WorldObject_City(new CityLevel("CARDIFF", new Vector2(100, 310))));
        }

        public WorldLevel(JSONTable template)
        {
            foreach (string cityCode in template.Keys)
            {
                objects.Add(new WorldObject_City(new CityLevel(template.getJSON(cityCode))));
            }
        }

        public void Run()
        {
            incomePerSecond = 0;
            foreach (WorldObject obj in objects)
            {
                obj.Run();
                incomePerSecond += obj.incomePerSecond;
            }
        }

        public void Update(InputState inputState)
        {
            inputState.UpdateMouseHover(objects);

            foreach (WorldObject obj in objects)
            {
                obj.Update(inputState);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.textures.mapBG, Vector2.Zero, Color.White);
            foreach(WorldObject obj in objects)
            {
                obj.Draw(spriteBatch);
            }

            Game1.instance.inventory.cityJustUnlocked = false;
        }
    }
}
