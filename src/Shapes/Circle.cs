using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public class Circle : Shape
    {
        private int _radius;

        public Circle (Color c, float x, float y, int radius, bool outline) : base(c, x, y, outline)
        {
            _radius = radius;
        }

        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
            }
        }

        public override void Draw()
        {
            if (Outline)
                SwinGame.DrawCircle(Color, X, Y, Radius);
            else
                SwinGame.FillCircle(Color, X, Y, Radius);
        }

        public override bool IsAt(Point2D pt)
        {
            return SwinGame.PointInCircle(pt, X, Y, Radius);
        }
    }
}
