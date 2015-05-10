using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public class Triangle : Shape
    {
        private int _x1, _y1, _x2, _y2, _x3, _y3;

        public Triangle(Color c, int x1, int y1, int x2, int y2, int x3, int y3, bool outline)
            : base(c, x1, y1, outline)
        {
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
            _x3 = x3;
            _y3 = y3;
        }

        public override void Draw()
        {
            if (Outline)
                SwinGame.DrawTriangle(Color, _x1, _y1, _x2, _y2, _x3, _y3);
            else
                SwinGame.FillTriangle(Color, _x1, _y1, _x2, _y2, _x3, _y3);
        }

        public override bool IsAt(Point2D pt)
        {
            return false;
        }
    }
}