using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRCEngine
{
    public class UIButtonStyle
    {
        public readonly UIButtonAppearance normal;
        public readonly UIButtonAppearance hover;
        public readonly UIButtonAppearance pressed;
        public readonly UIButtonAppearance disabled;

        public UIButtonStyle(UIButtonAppearance normal, UIButtonAppearance hover, UIButtonAppearance pressed, UIButtonAppearance disabled)
        {
            this.normal = normal;
            this.hover = hover;
            this.pressed = pressed;
            this.disabled = disabled;
        }
    }

    public class UIButtonAppearance
    {
        public readonly SpriteFont font;
        public readonly Color textColor;
        public readonly RichImage image;
        public readonly Vector2 textOffset;
        public readonly Color fillColor;

        public UIButtonAppearance(SpriteFont font, Color textColor, RichImage image, Color fillColor)
        {
            this.font = font;
            this.textColor = textColor;
            this.image = image;
            this.fillColor = fillColor;
        }

        public UIButtonAppearance(SpriteFont font, Color textColor, RichImage image, Color fillColor, Vector2 textOffset)
        {
            this.font = font;
            this.textColor = textColor;
            this.image = image;
            this.textOffset = textOffset;
        }

        public void Draw(SpriteBatch spriteBatch, string label, Rectangle frame)
        {
            image.Draw(spriteBatch, frame, fillColor);
//            MagicUI.Draw9Grid(spriteBatch, texture, frame, fillColor);
//            spriteBatch.Draw(texture, frame, fillColor);
            if (font != null)
            {
                Vector2 labelSize = font.MeasureString(label);
                spriteBatch.DrawString(font, label, new Vector2((float)Math.Floor(frame.Center.X + textOffset.X - labelSize.X / 2), (float)Math.Floor(frame.Center.Y + textOffset.Y - labelSize.Y / 2)), textColor);
            }
        }
    }

    public class UIButton :UIElement
    {
        public string label;
        public readonly Rectangle frame;
        public readonly UIButtonStyle styles;
        public readonly OnPressDelegate onPress;
        public delegate void OnPressDelegate();
        bool mouseInside;
        bool pressedInside;
        bool enabled = true;

        public static UIButtonStyle GetDefaultStyle(ContentManager Content)
        {
            SpriteFont font = Content.Load<SpriteFont>("Arial");
            RichImage normalImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d"), Color.White, "stretched9grid", 0, Rotation90.None));
            RichImage hoverImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d_hover"), Color.White, "stretched9grid", 0, Rotation90.None));
            RichImage pressedImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d_pressed"), Color.White, "stretched9grid", 0, Rotation90.None));

            return new UIButtonStyle(
                new UIButtonAppearance(font, Color.Black, normalImage, Color.White),
                new UIButtonAppearance(font, Color.Black, hoverImage, Color.White),
                new UIButtonAppearance(font, Color.Black, pressedImage, Color.White),
                new UIButtonAppearance(font, Color.Black, normalImage, Color.Gray)
            );
        }

        public UIButton(string label, Rectangle frame, UIButtonStyle styles, OnPressDelegate onPress)
        {
            this.label = label;
            this.frame = frame;
            this.styles = styles;
            this.onPress = onPress;
        }

        public override void Update(InputState inputState, Vector2 origin)
        {
            if (!enabled)
            {
                mouseInside = false;
                pressedInside = false;
                return;
            }

            mouseInside = frame.Contains(inputState.MousePos-origin);
            if(mouseInside && inputState.WasMouseLeftJustPressed())
            {
                pressedInside = true;
            }

            if (!inputState.mouseLeft.pressed)
            {
                if(mouseInside && pressedInside)
                {
                    onPress();
                }
                pressedInside = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            UIButtonAppearance currentStyle;
            if(!enabled)
            {
                currentStyle = styles.disabled;
            }
            else if(mouseInside)
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

            currentStyle.Draw(spriteBatch, label, new Rectangle(frame.X+(int)origin.X, frame.Y+(int)origin.Y, frame.Width, frame.Height));
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}
