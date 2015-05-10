using SwinGameSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petri
{
    public abstract class Shape
    {
        private Color _color;
        private float _x, _y;
        private bool _outline;

        public Shape(Color c, float x, float y, bool outline)
        {
            _x = x;
            _y = y;
            _color = c;
            _outline = outline;
        }

        public virtual Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }

        public virtual float X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public virtual float Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public bool Outline
        {
            get
            {
                return _outline;
            }
            set
            {
                _outline = value;
            }
        }

        public abstract void Draw();

        public abstract bool IsAt(Point2D pt);
    }
}
