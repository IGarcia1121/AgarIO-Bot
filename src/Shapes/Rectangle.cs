using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public class Rectangle : Shape
    {
        private int _width, _height;
        
        public Rectangle(Color c, float x, float y, int width, int height, bool outline) : base(c, x, y, outline)
        {
            _width = width;
            _height = height;
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

        public override void Draw()
        {
            if (Outline)
                SwinGame.DrawRectangle(Color, X, Y, Width, Height);
            else
                SwinGame.FillRectangle(Color, X, Y, Width, Height);
        }

        public override bool IsAt(Point2D pt)
        {
            return SwinGame.PointInRect(pt, X, Y, Width, Height);
        }
    }
}
