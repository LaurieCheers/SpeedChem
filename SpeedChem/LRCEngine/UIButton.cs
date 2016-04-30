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

        public UIButtonStyle(UIButtonAppearance normal, UIButtonAppearance hover, UIButtonAppearance pressed)
        {
            this.normal = normal;
            this.hover = hover;
            this.pressed = pressed;
        }
    }

    public class UIButtonAppearance
    {
        public readonly SpriteFont font;
        public readonly Color textColor;
        public readonly RichImage image;
        public readonly Vector2 textOffset;

        public UIButtonAppearance(SpriteFont font, Color textColor, RichImage image, Color fillColor)
        {
            this.font = font;
            this.textColor = textColor;
            this.image = image;
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
            image.Draw(spriteBatch, frame);
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
        public readonly string label;
        public readonly Rectangle frame;
        public readonly UIButtonStyle styles;
        public readonly OnPressDelegate onPress;
        public delegate void OnPressDelegate();
        bool mouseInside;
        bool pressedInside;

        public static UIButtonStyle GetDefaultStyle(ContentManager Content)
        {
            SpriteFont font = Content.Load<SpriteFont>("Arial");
            RichImage normalImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d"), Color.White, "stretched9grid", 0, Rotation90.None));
            RichImage hoverImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d_hover"), Color.White, "stretched9grid", 0, Rotation90.None));
            RichImage pressedImage = new RichImage(new RichImageLayer_Texture(Content.Load<Texture2D>("button3d_pressed"), Color.White, "stretched9grid", 0, Rotation90.None));

            return new UIButtonStyle(
                new UIButtonAppearance(font, Color.Black, normalImage, Color.White),
                new UIButtonAppearance(font, Color.Black, hoverImage, Color.White),
                new UIButtonAppearance(font, Color.Black, pressedImage, Color.White)
            );
        }

        public UIButton(string label, Rectangle frame, UIButtonStyle styles, OnPressDelegate onPress)
        {
            this.label = label;
            this.frame = frame;
            this.styles = styles;
            this.onPress = onPress;
        }

        public void Update(InputState inputState)
        {
            mouseInside = frame.Contains(inputState.MousePos);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            UIButtonAppearance currentStyle;
            if(mouseInside)
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

            currentStyle.Draw(spriteBatch, label, frame);
        }
    }
}
