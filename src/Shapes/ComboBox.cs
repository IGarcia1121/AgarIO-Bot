using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public class ComboBox : Shape
    {
        public event ItemSelectedHandler ItemSelected;
        public delegate void ItemSelectedHandler(string Item);

        private List<TextBox> _items;
        private LineSegment _seperator;
        private TextBox _text;
        private Triangle _tri;
        private Rectangle _clicker;
        private Font _font;
        private bool _dropdown;

        public ComboBox(Font font, float x, float y, int width, int height) : base(Color.Black, x, y, false)
        {
            _font = font;
            _items = new List<TextBox>();
            _text = new TextBox(font, x, y, width, height, -1);

            int triWidth = 8;
            int triHeight = 8;
            int trix = (int)x + width - (triWidth * 2);
            int triy = (int)y + height - (triHeight * 2) + 2;
            _tri = new Triangle(Color.Black, trix, triy, trix + triWidth, triy, trix + (triWidth / 2), triy + triHeight, false);

            _seperator = new LineSegment();
            _seperator.StartPoint = new Point2D { X = trix - 5, Y = triy - 5 };
            _seperator.EndPoint = new Point2D { X = trix - 5, Y = y + height - 2 };

            _clicker = new Rectangle(Color.LightGray, _seperator.StartPoint.X, _seperator.StartPoint.Y, triWidth * 2 + 4, height - 2, false);

            _text.Text = "Select an option...";
        }

        public string Text
        {
            get
            {
                return _text.Text;
            }
            set
            {
                _text.Text = value;
            }
        }

        public bool DroppedDown
        {
            get
            {
                return _dropdown;
            }
            set
            {
                _dropdown = value;
            }
        }

        public override void Draw()
        {
            //Don't allow modification of textbox
            _text.Focused = false;
            _text.Draw();
            _clicker.Draw();
            _tri.Draw();
            SwinGame.DrawLineOnScreen(Color.Black, _seperator);

            if(_dropdown)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    TextBox box = _items[i];
                    box.Y = _text.Y + (box.Height * (i + 1));
                    box.Focused = false;
                    box.Draw();

                    if (box.IsAt(SwinGame.MousePosition()) && SwinGame.MouseClicked(MouseButton.LeftButton))
                    {
                        _text.Text = box.Text;

                        if (ItemSelected != null)
                        {
                            ItemSelected(box.Text);
                            break;
                        }
                    }
                }
            }

            if (_text.IsAt(SwinGame.MousePosition()) && SwinGame.MouseClicked(MouseButton.LeftButton))
            {
                _dropdown = !_dropdown;
            }
            else if (!_text.IsAt(SwinGame.MousePosition()) && SwinGame.MouseClicked(MouseButton.LeftButton))
            {
                _dropdown = false;
            }
        }

        public void AddItem(string Item)
        {
            TextBox box = new TextBox(_font, X, Y, _text.Width, _text.Height, -1);
            box.Text = Item;
            _items.Add(box);
        }

        public override bool IsAt(Point2D pt)
        {
            throw new NotImplementedException();
        }
    }
}
