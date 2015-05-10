using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public class TextBox : Shape
    {
        public Dictionary<string, string> CharLookup = new Dictionary<string, string>
        {
            {"Space", " "},
            {"Period", "."},
            {"Semicolon", ":"}, //Can't type ; because no shift
            {"Colon", ":"}
        };

        private bool _focused;
        private Font _font;
        private int _width, _height, _maxLen;
        private uint _ticks;
        private string _text;
        public Rectangle _rect, _caret, _background;

        public TextBox(Font font, float x, float y, int width, int height, int maxLen) : base(Color.Black, x, y, false)
        {
            _font = font;
            _width = width;
            _height = height;
            _maxLen = maxLen;
            _text = "";
            _ticks = 0;

            _rect = new Rectangle(Color.Black, X, Y, width, height, true);
            _caret = new Rectangle(Color.Black, X + 2, Y + height - 4, 5, 2, false);
            _background = new Rectangle(Color.White, X, Y, width, height, false);
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _caret.X = X + 2 + _font.TextWidth(value);
                _text = value;
            }
        }

        public bool Focused
        {
            get
            {
                return _focused;
            }
            set
            {
                _focused = value;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public override float Y
        {
            get
            {
                return base.Y;
            }
            set
            {
                _rect.Y = value;
                _caret.Y = value + _height - 4;
                _background.Y = value;
                base.Y = value;
            }
        }

        public List<KeyCode> KeyPressed()
        {
            List<KeyCode> keyCodeList = new List<KeyCode>();
            var enumValues = Enum.GetValues(typeof(KeyCode));

            foreach (KeyCode code in enumValues)
            {
                if (SwinGame.KeyTyped(code))
                    keyCodeList.Add(code);
            }

            return keyCodeList;
        }

        public override void Draw()
        {
            _background.Draw();

            if (Focused)
            {
                List<KeyCode> keyCodes = KeyPressed();
                foreach (KeyCode code in keyCodes)
                {
                    string Key = SwinGame.KeyName(code);
                    if (Key.Length == 1)
                    {
                        Text += Key;
                    }
                    else if (CharLookup.ContainsKey(Key))
                    {
                        Text += CharLookup[Key];
                    }
                    else if (code == KeyCode.vk_BACKSPACE && Text.Length > 0)
                    {
                        Text = Text.Remove(Text.Length - 1, 1);
                    }
                    else
                    {

                    }

                    if (Text.Length > _maxLen && _maxLen != -1)
                    {
                        Text = Text.Remove(_maxLen, Text.Length - _maxLen);
                    }

                    _caret.X = X + 2 + _font.TextWidth(Text);
                }

                if (SwinGame.GetTicks() - _ticks > 300)
                {
                    if (SwinGame.GetTicks() - _ticks > 600)
                        _ticks = SwinGame.GetTicks();

                    _caret.Draw();
                }
            }

            if (IsAt(SwinGame.MousePosition()))
            {
                Focused = true;
            }
            else
            {
                Focused = false;
            }

            _rect.Draw();
            SwinGame.DrawTextOnScreen(_text, Color.Black, _font, (int)X + 2, (int)Y + 2);
        }

        public override bool IsAt(Point2D pt)
        {
            return _rect.IsAt(pt);
        }
    }
}
