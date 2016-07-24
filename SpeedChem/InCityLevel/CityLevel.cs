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
    abstract class UnlockRule
    {
        public bool unlocked { get { return objectsUnlocked == null && uiUnlocked == null; } }

        List<CityObject> objectsUnlocked;
        List<UIElement> uiUnlocked;
        List<Weapon> weapons;

        public UnlockRule(List<UIElement> uiUnlocked, List<CityObject> objectsUnlocked, List<Weapon> weapons)
        {
            this.uiUnlocked = uiUnlocked;
            this.objectsUnlocked = objectsUnlocked;
            this.weapons = weapons;
        }

        public UnlockRule(List<CityObject> objectsUnlocked)
        {
            this.objectsUnlocked = objectsUnlocked;
        }

        public UnlockRule(List<UIElement> uiUnlocked)
        {
            this.uiUnlocked = uiUnlocked;
        }

        public UnlockRule(CityObject obj)
        {
            this.objectsUnlocked = new List<CityObject>() { obj };
        }

        public void Unlock(CityLevel metaGame)
        {
            if (objectsUnlocked != null)
            {
                foreach (CityObject obj in objectsUnlocked)
                {
                    metaGame.AddObject(obj);
                }
            }
            if (uiUnlocked != null)
            {
                foreach (UIElement ui in uiUnlocked)
                {
                    metaGame.AddUI(ui);
                }
            }
            if(weapons != null)
            {
                foreach(Weapon w in weapons)
                {
                    Game1.instance.inventory.UnlockWeapon(w);
                }
            }
            objectsUnlocked = null;
            uiUnlocked = null;
        }

        public void Update(CityLevel metaGame)
        {
            if (Test(metaGame))
                Unlock(metaGame);
        }
        public abstract bool Test(CityLevel metaGame);
    }

    class UnlockRule_Money: UnlockRule
    {
        int minMoney;
        public UnlockRule_Money(int minMoney, List<CityObject> objects): base(objects)
        {
            this.minMoney = minMoney;
        }
        public UnlockRule_Money(int minMoney, List<UIElement> ui) : base(ui)
        {
            this.minMoney = minMoney;
        }
        public UnlockRule_Money(int minMoney, CityObject obj) : base(obj)
        {
            this.minMoney = minMoney;
        }
        public override bool Test(CityLevel metaGame)
        {
            return Game1.instance.inventory.money >= minMoney;
        }
    }

    class UnlockRule_Output: UnlockRule
    {
        CityObject watching;
        public UnlockRule_Output(CityObject watching, List<CityObject> objects): base(objects)
        {
            this.watching = watching;
        }
        public UnlockRule_Output(CityObject watching, List<UIElement> ui) : base(ui)
        {
            this.watching = watching;
        }
        public UnlockRule_Output(CityObject watching, CityObject obj) : base(obj)
        {
            this.watching = watching;
        }
        public UnlockRule_Output(CityObject watching, List<UIElement> ui, List<CityObject> objects, List<Weapon> weapons) : base(ui, objects, weapons)
        {
            this.watching = watching;
        }
        public override bool Test(CityLevel metaGame)
        {
            return watching.didOutput;
        }
    }

    public class CityUIBlackboard
    {
        public CityLevel cityLevel;
        public CityObject selectedObject;
        public CityObject draggingOntoObject;
        public InputState inputState;
    }

    public class CityLevel: SpeedChemScreen, UIMouseResponder
    {
        List<CityObject> objects = new List<CityObject>();
        List<CityObject> nextObjects = null;
        UIContainer ui;
        public CityUIBlackboard blackboard = new CityUIBlackboard();
        //Vector2 nextSiloPos = new Vector2(100,300);
        //Dictionary<ChemicalSignature, ChemicalSilo> chemicals = new Dictionary<ChemicalSignature, ChemicalSilo>();
        int nextFactoryPrice = 16;
        int cuttingBeamPrice = 400;
//        Dictionary<string, UIElement> unlockableUI;
        UIBuyButton newFactoryButton;
        UIBuyButton newBigFactoryButton;
        UIButton newCentrifugeButton;
        UIButton newSiloButton;
        UIButton buyCuttingBeamButton;
        List<UnlockRule> unlockRules = new List<UnlockRule>();
        public readonly string name;
        public readonly Vector2 pos;
        public readonly int price;
        public float incomePerSecond = 0;
        public readonly bool isTutorial;
        public bool isComplete;
        public bool isNew;

        public CityLevel(JSONTable template)
        {
            this.name = template.getString("name");
            this.pos = template.getVector2("pos");
            this.price = template.getInt("price");
            this.isTutorial = template.getBool("isTutorial", false);
            blackboard.cityLevel = this;

            InitUI();

            isNew = true;

            Dictionary<string, CityObject> objectsByName = new Dictionary<string, CityObject>();

            JSONTable unlockTable = template.getJSON("unlocks", null);
            if (unlockTable != null)
            {
                foreach (string unlockName in unlockTable.Keys)
                {
                    JSONTable unlockTemplates = unlockTable.getJSON(unlockName);
                    List<CityObject> newObjects = new List<CityObject>();
                    List<UIElement> newUI = new List<UIElement>();
                    List<Weapon> newWeapons = new List<Weapon>();
                    foreach (string objectName in unlockTemplates.Keys)
                    {
                        /*if(unlockableUI.ContainsKey(objectName))
                        {
                            newUI.Add(unlockableUI[objectName]);
                        }
                        else*/
                        if (Game1.instance.inventory.unlockableWeapons.ContainsKey(objectName))
                        {
                            newWeapons.Add(Game1.instance.inventory.unlockableWeapons[objectName]);
                        }
                        else
                        {
                            CityObject newObj = CityObject.FromTemplate(this, unlockTemplates.getJSON(objectName));
                            objectsByName[objectName] = newObj;
                            newObjects.Add(newObj);
                        }
                    }

                    if (unlockName == "start")
                    {
                        foreach (CityObject obj in newObjects)
                        {
                            objects.Add(obj);

                            //tutorial hack
                            if (obj is ChemicalFactory)
                                blackboard.selectedObject = obj;
                        }
                        foreach (UIElement element in newUI)
                        {
                            ui.Add(element);
                        }
                        foreach(Weapon w in newWeapons)
                        {
                            Game1.instance.inventory.UnlockWeapon(w);
                        }
                    }
                    else
                    {
                        unlockRules.Add( new UnlockRule_Output(objectsByName[unlockName], newUI, newObjects, newWeapons) );
                    }
                }

                // postprocess to add pipes
                foreach (string unlockName in unlockTable.Keys)
                {
                    JSONTable unlockTemplates = unlockTable.getJSON(unlockName);
                    foreach (string objectName in unlockTemplates.Keys)
                    {
                        //if (unlockableUI.ContainsKey(objectName) ||
                        if (Game1.instance.inventory.unlockableWeapons.ContainsKey(objectName))
                            continue;

                        JSONTable objectTemplate = unlockTemplates.getJSON(objectName);
                        JSONArray pipeTargets = objectTemplate.getArray("pipes", null);
                        if (pipeTargets != null)
                        {
                            foreach (string pipeTarget in pipeTargets.asStrings())
                            {
                                CityObject sourceObject = objectsByName[objectName];
                                CityObject targetObject = objectsByName[pipeTarget];
                                OutputPipe pipe = sourceObject.pipes.Last();
                                pipe.ConnectTo(targetObject.GetNearestSocket(targetObject.bounds.XY));
                                pipe.movable = false;
                            }
                        }
                    }
                }
            }
        }

        public void InitUI()
        {
            ui = new UIContainer();

            if (Game1.instance.inventory.hasWorldMap)
            {
                ui.Add(new UIButton("World Map", new Rectangle(600, 10, 190, 40), Game1.buttonStyle, button_GoToMap));
            }

            int buttonY = 70;

            if (Game1.instance.inventory.hasFactoryBlueprint)
            {
                newFactoryButton = new UIBuyButton("Build Factory", nextFactoryPrice, new Rectangle(600, buttonY, 190, 40), Game1.buttonStyle, button_SpawnFactory);
                ui.Add(newFactoryButton);
                buttonY += 50;
            }
            if (Game1.instance.inventory.hasBigFactoryBlueprint)
            {
                newBigFactoryButton = new UIBuyButton("Build Big Factory", nextFactoryPrice, new Rectangle(600, buttonY, 190, 40), Game1.buttonStyle, button_SpawnBigFactory);
                ui.Add(newBigFactoryButton);
                buttonY += 50;
            }
            //newFactoryButton = new UIBuyButton("Build Factory", 100, new Rectangle(600, 100, 170, 40), Game1.buttonStyle, button_SpawnFactory);
            if (Game1.instance.inventory.hasSiloBlueprint)
            {
                newSiloButton = new UIBuyButton("Build Silo", 150, new Rectangle(600, buttonY, 190, 40), Game1.buttonStyle, button_SpawnSilo);
                ui.Add(newSiloButton);
                buttonY += 50;
            }
            if (Game1.instance.inventory.hasCentrifugeBlueprint)
            {
                newCentrifugeButton = new UIBuyButton("Build Centrifuge", 150, new Rectangle(600, buttonY, 190, 40), Game1.buttonStyle, button_SpawnCentrifuge);
                ui.Add(newCentrifugeButton);
                buttonY += 50;
            }

            /*            unlockableUI = new Dictionary<string, UIElement>()
                        {
                            { "BUY_FACTORY", newFactoryButton },
                            { "BUY_BIG_FACTORY", newBigFactoryButton },
                            { "BUY_SILO", newSiloButton },
                            { "BUY_CENTRIFUGE", newCentrifugeButton },
                        };*/

#if DEBUG
            ui.Add(new UIButton("Cheat:Unlocks", new Rectangle(600, 370, 170, 40), Game1.buttonStyle, button_Unlocks));
            ui.Add(new UIButton("Cheat:Loadsamoney", new Rectangle(600, 420, 170, 40), Game1.buttonStyle, button_CheatMoney));
#endif
        }

        public void AddObject(CityObject obj)
        {
            objects.Add(obj);
        }

        public void AddObjectDeferred(CityObject obj)
        {
            if (nextObjects == null)
                nextObjects = objects.ToList();

            nextObjects.Add(obj);
        }

        public void RemoveObject(CityObject obj)
        {
            objects.Remove(obj);
        }

        public void RemoveObjectDeferred(CityObject obj)
        {
            if (nextObjects == null)
                nextObjects = objects.ToList();

            nextObjects.Remove(obj);
        }

        public void AddUI(UIElement element)
        {
            ui.Add(element);
        }

        public void button_SpawnFactory()
        {
            Vector2 factoryPos = new Vector2(500, 80);
            objects.Add(new ChemicalFactory(this, factoryPos, true, false));
            nextFactoryPrice = (int)(nextFactoryPrice * 5);
            newFactoryButton.price = nextFactoryPrice;
        }

        public void button_SpawnBigFactory()
        {
            Vector2 factoryPos = new Vector2(500, 80);
            objects.Add(new ChemicalFactory(this, factoryPos, true, true));
            nextFactoryPrice = (int)(nextFactoryPrice * 5);
            newFactoryButton.price = nextFactoryPrice;
        }

        public void button_SpawnSilo()
        {
            objects.Add(new ChemicalSilo(this, new Vector2(500, 80)));
        }

        public void button_SpawnCentrifuge()
        {
            objects.Add(new Centrifuge(this, new Vector2(500, 160)));
        }

        public void button_GoToMap()
        {
            Game1.instance.ViewWorld();
        }

        public void button_CheatMoney()
        {
            Game1.instance.inventory.GainMoney(1000000, Vector2.Zero, this);
            Game1.instance.inventory.GainCrystals(100);
        }

        public void button_Unlocks()
        {
            foreach(CityObject obj in objects)
            {
                if(obj is ChemicalFactory)
                {
                    (obj as ChemicalFactory).didOutput = true;
                }
                else if (obj is ChemicalOutbox)
                {
                    (obj as ChemicalOutbox).didOutput = true;
                }
                else if (obj is CrystalOutbox)
                {
                    (obj as CrystalOutbox).didOutput = true;
                }
            }
        }

        public void Run()
        {
            incomePerSecond = 0;
            foreach (CityObject obj in objects)
            {
                obj.Run();

                ChemicalFactory factory = obj as ChemicalFactory;
                if (factory != null)
                {
                    incomePerSecond += factory.incomePerSecond;
                }

                foreach (OutputPipe pipe in obj.pipes)
                {
                    pipe.Run();
                }

            }
        }

        public UIMouseResponder GetMouseHover(Vector2 mousePos)
        {
            UIMouseResponder result = ui.GetMouseHover(mousePos);
            if (result != null) return result;

            for (int Idx = objects.Count - 1; Idx >= 0; --Idx)
            {
                result = objects[Idx].GetOverlayMouseHover(mousePos);
                if (result != null)
                    return result;
            }

            for (int Idx = objects.Count - 1; Idx >= 0; --Idx)
            {
                result = objects[Idx].GetPipeMouseHover(mousePos);
                if (result != null)
                    return result;
            }

            for (int Idx = objects.Count - 1; Idx >= 0; --Idx)
            {
                result = objects[Idx].GetMouseHover(mousePos);
                if (result != null)
                    return result;
            }

            return this;
        }

        public void Update(InputState inputState)
        {
            blackboard.inputState = inputState;
            blackboard.draggingOntoObject = null;

            isComplete = true;

            for(int Idx = unlockRules.Count-1; Idx >= 0; Idx--)
            {
                UnlockRule rule = unlockRules[Idx];
                rule.Update(this);
                if (rule.unlocked)
                {
                    int lastIdx = unlockRules.Count - 1;
                    unlockRules[Idx] = unlockRules[lastIdx];
                    unlockRules.RemoveAt(lastIdx);
                }
                else
                {
                    isComplete = false;
                }
            }

            inputState.hoveringElement = GetMouseHover(inputState.MousePos);

            foreach (CityObject obj in objects)
            {
                obj.Update(blackboard);

                foreach(OutputPipe pipe in obj.pipes)
                {
                    pipe.Update(this, blackboard);
                }
            }

            ui.Update(inputState);

            if (inputState.WasMouseLeftJustReleased()
                && blackboard.selectedObject != null)
            {
                if (blackboard.draggingOntoObject != null)
                {
                    blackboard.selectedObject.DisconnectPipes();
                    objects.Remove(blackboard.selectedObject);
                    (blackboard.draggingOntoObject as ChemicalFactory).AddCores((blackboard.selectedObject as ChemicalFactory).numCores);
                    blackboard.draggingOntoObject = null;
                }

                // when you click on the field, clear the selected object
                if(inputState.hoveringElementMouseDown == this)
                    blackboard.selectedObject = null;
            }

            if(nextObjects != null)
            {
                objects = nextObjects;
                nextObjects = null;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureCache.shelf, new Rectangle(0, 0, 800, 600), Color.White);

            foreach (CityObject obj in objects)
            {
                obj.Draw(spriteBatch, blackboard);
            }

            foreach (CityObject obj in objects)
            {
                foreach (OutputPipe pipe in obj.pipes)
                {
                    pipe.Draw(spriteBatch);
                }
            }

            foreach (CityObject obj in objects)
            {
                obj.DrawUI(spriteBatch, blackboard);
            }

            if (blackboard.selectedObject != null && blackboard.selectedObject.dragging)
            {
                foreach (CityObject obj in objects)
                {
                    obj.DrawDraggingUI(spriteBatch, blackboard);
                }
            }

            ui.Draw(spriteBatch);
        }

        /*
        public void ProduceChemical(ChemicalSignature signature)
        {
            if(!chemicals.ContainsKey(signature))
            {
                ChemicalSilo newSilo = new ChemicalSilo(signature, amount, TextureCache.silo, nextSiloPos, new Vector2(32, 32));
                objects.Add(newSilo);
                chemicals[signature] = newSilo;
                nextSiloPos.X += 50;
            }
            else
            {
                chemicals[signature].amount++;
            }
        }
        */

        public CityObject GetObjectAt(Vector2 pos)
        {
            foreach(CityObject obj in objects)
            {
                if (obj.bounds.Contains(pos))
                    return obj;
            }

            return null;
        }
    }
}
