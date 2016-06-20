﻿using LRCEngine;
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
        Dictionary<string, UIElement> unlockableUI;
        UIBuyButton newFactoryButton;
        UIButton newSiloButton;
        UIButton buyCuttingBeamButton;
        List<UnlockRule> unlockRules = new List<UnlockRule>();
        public readonly string name;
        public readonly Vector2 pos;
        public readonly int price;
        public float incomePerSecond = 0;

        public CityLevel(JSONTable template)
        {
            this.name = template.getString("name");
            this.pos = template.getVector2("pos");
            this.price = template.getInt("price");
            blackboard.cityLevel = this;

            Init();

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
                        if(unlockableUI.ContainsKey(objectName))
                        {
                            newUI.Add(unlockableUI[objectName]);
                        }
                        else if(Game1.instance.inventory.unlockableWeapons.ContainsKey(objectName))
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
                        if (unlockableUI.ContainsKey(objectName) || Game1.instance.inventory.unlockableWeapons.ContainsKey(objectName))
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
                                pipe.ConnectTo(targetObject.pipeSocket);
                                pipe.movable = false;
                            }
                        }
                    }
                }
            }
        }

        public CityLevel(string name, Vector2 pos)
        {
            this.name = name;
            this.pos = pos;
            blackboard.cityLevel = this;

            int nextInputX = 100;
            int nextOutputX = 20;
            int inputSpacingX = 100;
            int outputSpacingX = 87;

            //==========================
            // tutorial factory

            ChemicalInbox tutorialInbox = new ChemicalInbox(this, new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.WHITE }), 0, new Vector2(nextInputX, 30));
            objects.Add(tutorialInbox);

            ChemicalFactory tutorialFactory = new ChemicalFactory(this, new Vector2(nextOutputX, 310), false);
            tutorialFactory.canDrag = false;
            objects.Add(tutorialFactory);

            ChemicalOutbox tutorialOutbox = new ChemicalOutbox(
                this, new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.WHITE }), 1, new Vector2(nextOutputX, 380)
            );
            objects.Add(tutorialOutbox);

            tutorialFactory.unlimitedPipes = false;
            tutorialInbox.pipes.First().ConnectTo(tutorialFactory.pipeSocket);
            tutorialInbox.pipes.First().movable = false;
            tutorialFactory.pipes.First().ConnectTo(tutorialOutbox.pipeSocket);
            tutorialFactory.pipes.First().movable = false;
            blackboard.selectedObject = tutorialFactory;

            //=======================

            Init();

            //=======================

            nextInputX += inputSpacingX;
            nextOutputX += outputSpacingX;

            ChemicalFactory factory2 = new ChemicalFactory(this, new Vector2(nextOutputX, 310), false);
            ChemicalOutbox outbox2 = new ChemicalOutbox(this, new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.BLUE, ChemicalElement.BLUE, }), 18, new Vector2(nextOutputX, 380));
            unlockRules.Add(new UnlockRule_Output(tutorialFactory,
                new List<CityObject>
                {
                    new ChemicalInbox(this, new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.BLUE }), 8, new Vector2(nextInputX, 30) ),
                    factory2,
                    outbox2
                }
            ));
            factory2.unlimitedPipes = false;
            factory2.pipes.First().ConnectTo(outbox2.pipeSocket);
            factory2.pipes.First().movable = false;

            //=======================

            nextOutputX += outputSpacingX;
            ChemicalFactory factory3 = new ChemicalFactory(this, new Vector2(nextOutputX, 310), false);
            ChemicalOutbox outbox3 = new ChemicalOutbox
            (
                this,
                new ChemicalSignature(3, new ChemicalElement[] { ChemicalElement.BLUE, ChemicalElement.WHITE, ChemicalElement.BLUE }),
                44,
                new Vector2(nextOutputX, 380)
            );
            factory3.unlimitedPipes = false;
            factory3.pipes.First().ConnectTo(outbox3.pipeSocket);
            factory3.pipes.First().movable = false;

            unlockRules.Add(new UnlockRule_Output(factory2, new List<CityObject>() { factory3, outbox3 }));

            //=======================

            nextInputX += inputSpacingX;
            nextOutputX += outputSpacingX;
            ChemicalOutbox outbox4 = new ChemicalOutbox
            (
                this,
                new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.GLASS, ChemicalElement.BLUE, ChemicalElement.BLUE, ChemicalElement.GLASS }),
                280,
                new Vector2(nextOutputX, 380)
            );
            unlockRules.Add(new UnlockRule_Output(factory3,
                new List<UIElement>() {
                    newFactoryButton,
                    //new UIButton("New Silo", new Rectangle(600, 150, 120, 40), Game1.buttonStyle, button_SpawnSilo)
                },
                new List<CityObject>()
                {
                    new ChemicalInbox(this, new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.GLASS }), 80, new Vector2(nextInputX, 30)),
                    outbox4
                },
                new List<Weapon>() { }
            ));

            //========================

            nextInputX += inputSpacingX;
            nextOutputX += outputSpacingX;
            CrystalOutbox outbox5 = new CrystalOutbox
            (
                this,
                new ChemicalSignature(3, new ChemicalElement[] { ChemicalElement.NONE, ChemicalElement.WHITE, ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.RED, ChemicalElement.RED, ChemicalElement.NONE, ChemicalElement.WHITE, ChemicalElement.NONE, }),
                new Vector2(nextOutputX, 410)
            );
            unlockRules.Add(new UnlockRule_Output(outbox4,
                new List<CityObject>()
                {
                    new ChemicalInbox(this, new ChemicalSignature(3, new ChemicalElement[] { ChemicalElement.RED,ChemicalElement.RED,ChemicalElement.RED }), 2000, new Vector2(nextInputX, 30)),
                    outbox5
                }
            ));

            //========================

            nextInputX += inputSpacingX;
            nextOutputX += outputSpacingX;
            unlockRules.Add(new UnlockRule_Output(outbox5,
                new List<UIElement>()
                {
                    buyCuttingBeamButton
                },
                new List<CityObject>()
                {
                    new ChemicalOutbox
                    (
                        this,
                        new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.RED }),
                        2600,
                        new Vector2(nextOutputX, 380)
                    )
                },
                new List<Weapon>()
                {
                }
            ));

            nextOutputX += outputSpacingX;
/*            unlockRules.Add(new UnlockRule_Money(600,
                new List<MetaGameObject>()
                {
                    new ChemicalOutbox
                    (
                        new ChemicalSignature(4, new ChemicalElement[] {
                            ChemicalElement.NONE, ChemicalElement.WHITE, ChemicalElement.GLASS, ChemicalElement.WHITE,
                            ChemicalElement.GLASS, ChemicalElement.WHITE, ChemicalElement.GLASS, ChemicalElement.WHITE
                        }),
                        515,
                        new Vector2(nextOutputX, 380)
                    )
                }
            ));*/

            /*
            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox(new ChemicalSignature(2, new ChemicalElement[] { ChemicalElement.WHITE, ChemicalElement.RED }), 80, new Vector2(nextInputX, 30)));
            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox(new ChemicalSignature(1, new ChemicalElement[] { ChemicalElement.GLASS }), 10000, new Vector2(nextInputX, 30)));

            nextInputX += inputSpacingX;
            objects.Add(new ChemicalInbox
            (
                new ChemicalSignature(1, new ChemicalElement[] {
                    ChemicalElement.BLUE,
                    ChemicalElement.BLUE,
                }),
                0,
                new Vector2(nextInputX, 30)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(1, new ChemicalElement[] {
                    ChemicalElement.BLUE,
                    ChemicalElement.BLUE,
                }),
                15,
                new Vector2(nextOutputX, 150)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.NONE,ChemicalElement.WHITE,ChemicalElement.NONE,
                    ChemicalElement.WHITE,ChemicalElement.RED,ChemicalElement.WHITE,
                    ChemicalElement.NONE,ChemicalElement.WHITE,ChemicalElement.NONE,
                }),
                FactoryCommandType.GAINCRYSTAL,
                new Vector2(nextOutputX, 180)
            ));


            nextOutputX = 60;


            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.WHITE, ChemicalElement.BLUE, ChemicalElement.WHITE
                }),
                300,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.RED,
                    ChemicalElement.RED, ChemicalElement.RED, ChemicalElement.NONE,
                    ChemicalElement.NONE, ChemicalElement.RED, ChemicalElement.NONE
                }),
                1000,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(2, new ChemicalElement[] {
                    ChemicalElement.GLASS, ChemicalElement.BLUE,
                    ChemicalElement.BLUE, ChemicalElement.GLASS,
                }),
                28000,
                new Vector2(nextOutputX, 350)
            ));

            nextOutputX += outputSpacingX;
            objects.Add(new ChemicalFactory
            (
                new ChemicalSignature(3, new ChemicalElement[] {
                    ChemicalElement.GLASS, ChemicalElement.NONE, ChemicalElement.GLASS,
                    ChemicalElement.BLUE, ChemicalElement.GLASS, ChemicalElement.BLUE,
                }),
                46000,
                new Vector2(nextOutputX, 350)
            ));
            */

            //            ChemicalFactory tutorialFactory = new ChemicalFactory(new Vector2(100, 200));
            //            objects.Add(tutorialFactory);

            //            tutorialInbox.pipes.First().ConnectTo(tutorialFactory.pipeSocket);
            //            tutorialFactory.pipes.First().ConnectTo(tutorialOutbox.pipeSocket);

            //objects.Add(new ChemicalFactory(new Vector2(400, 200)));

            //            selectedObject = tutorialFactory;
        }

        public void Init()
        {
            ui = new UIContainer();

            newFactoryButton = new UIBuyButton("Build Factory", nextFactoryPrice, new Rectangle(600, 100, 170, 40), Game1.buttonStyle, button_SpawnFactory);
            //newFactoryButton = new UIBuyButton("Build Factory", 100, new Rectangle(600, 100, 170, 40), Game1.buttonStyle, button_SpawnFactory);
            newSiloButton = new UIBuyButton("Build Silo", 150, new Rectangle(600, 150, 170, 40), Game1.buttonStyle, button_SpawnSilo);

            unlockableUI = new Dictionary<string, UIElement>()
            {
                { "BUY_FACTORY", newFactoryButton },
                { "BUY_SILO", newSiloButton },
            };

            ui.Add(new UIButton("Back to Map", new Rectangle(600, 10, 170, 40), Game1.buttonStyle, button_GoToMap));

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
            Vector2 factoryPos = new Vector2(50, 200);
            objects.Add(new ChemicalFactory(this, factoryPos, true));
            nextFactoryPrice = (int)(nextFactoryPrice * 5);
            newFactoryButton.price = nextFactoryPrice;
        }

        public void button_SpawnSilo()
        {
            objects.Add(new ChemicalSilo(this, new Vector2(50, 200)));
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
                ChemicalFactory factory = obj as ChemicalFactory;
                if (factory != null)
                {
                    factory.Run();
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
            spriteBatch.Draw(TextureCache.grass, new Rectangle(0, 0, 800, 600), Color.White);

            foreach (CityObject obj in objects)
            {
                foreach(OutputPipe pipe in obj.pipes)
                {
                    pipe.Draw(spriteBatch);
                }
            }

            foreach (CityObject obj in objects)
            {
                obj.Draw(spriteBatch, blackboard);
            }

            if(blackboard.selectedObject != null && blackboard.selectedObject.dragging)
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
