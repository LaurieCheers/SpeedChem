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
    class UIWeaponSelectable : UIButton
    {
        Weapon weapon;
        OnWeaponDelegate onWeaponPress;

        public delegate void OnWeaponDelegate(Weapon w);

        public UIWeaponSelectable(Weapon weapon, Rectangle frame, UIButtonStyle style, OnWeaponDelegate onWeaponPress) : base(weapon.name, frame, style, null)
        {
            this.weapon = weapon;
            this.onWeaponPress = onWeaponPress;
        }

        protected override void Pressed()
        {
            onWeaponPress(weapon);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            base.Draw(spriteBatch, origin);
            spriteBatch.Draw(weapon.texture, frame.XY() + origin + new Vector2(5, 5), Color.White);
        }
    }

    class UIWeaponSlot : UIButton
    {
        Inventory.WeaponSlot slot;
        List<Weapon> availableWeapons;
        UIContainer dropDown = new UIContainer();
        bool isOpen;
        Texture2D buttonIcon;

        public UIWeaponSlot(Inventory.WeaponSlot slot, List<Weapon> availableWeapons, Texture2D buttonIcon, Rectangle frame, UIButtonStyle style) : base("", frame, style, null)
        {
            onPress = slot_Clicked;
            this.slot = slot;
            this.buttonIcon = buttonIcon;
            this.availableWeapons = availableWeapons;
        }

        static UIButtonStyle weaponSlotStyle;

        void slot_Clicked()
        {
            isOpen = !isOpen;

            if(weaponSlotStyle == null)
            {
                weaponSlotStyle = new UIButtonStyle(
                    new UIButtonAppearance(Game1.font, Color.White, TextureCache.castIronButton, Color.White, new Vector2(25, 0)),
                    new UIButtonAppearance(Game1.font, Color.White, TextureCache.castIronButton_hover, Color.White, new Vector2(25, 0)),
                    new UIButtonAppearance(Game1.font, Color.White, TextureCache.castIronButton_pressed, Color.White, new Vector2(25, 1)),
                    new UIButtonAppearance(Game1.font, Color.White, TextureCache.castIronButton, Color.White, new Vector2(25, 0))
                );
            }

            const int BUTTONHEIGHT = 50;
            dropDown.RemoveAll();
            Vector2 nextSelectablePos = new Vector2(frame.X, frame.Y - BUTTONHEIGHT * (availableWeapons.Count()-1));
            foreach(Weapon w in availableWeapons)
            {
                if (w != slot.weapon)
                {
                    dropDown.Add(new UIWeaponSelectable(w, new Rectangle((int)nextSelectablePos.X, (int)nextSelectablePos.Y, 175, BUTTONHEIGHT), weaponSlotStyle, button_SelectWeapon));
                    nextSelectablePos.Y += BUTTONHEIGHT;
                }
            }
        }

        public void button_SelectWeapon(Weapon w)
        {
            slot.weapon = w;
        }

        public override UIMouseResponder GetMouseHover(Vector2 localMousePos)
        {
            if (frame.Contains(localMousePos))
                return this;

            if (isOpen)
                return dropDown.GetMouseHover(localMousePos);

            return null;
        }

        public override void Update(InputState inputState, Vector2 origin)
        {
            if (isOpen)
            {
                dropDown.Update(inputState, origin);
                if (inputState.WasMouseLeftJustReleased())
                {
                    isOpen = false;
                }
            }
            else
            {
                base.Update(inputState, origin);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            UIButtonAppearance currentStyle;
            if (mouseInside)
            {
                if (pressedInside)
                    currentStyle = styles.pressed;
                else
                    currentStyle = styles.hover;
            }
            else
            {
                currentStyle = styles.normal;
            }

            currentStyle.Draw(spriteBatch, slot.weapon.name, new Rectangle(frame.X + (int)origin.X, frame.Y + (int)origin.Y, frame.Width, frame.Height));
            spriteBatch.Draw(slot.weapon.texture, new Vector2(frame.X + (int)origin.X + 5, frame.Y + (int)origin.Y + 5), Color.White);

            if(isOpen)
            {
                dropDown.Draw(spriteBatch, origin);
            }

            spriteBatch.Draw(buttonIcon, new Rectangle(frame.Right - 24, frame.Top + 4, 16, 16), Color.White);
        }
    }
}
