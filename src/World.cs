using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using SwinGameSDK;

namespace Petri
{
    public class World : Shape
    {
        public event PlayerDiedHandler PlayerDied;
        public delegate void PlayerDiedHandler();

        private int _gridSize = 75;
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        private uint _playerId;

        private double _cameraX;
        private double _cameraY;

        private List<Blob> _blobs;
        private Dictionary<uint, string> _nameCache;

        public World(double x, double y, double Width, double Height) : base (Color.Black, 0, 0, false)
        {
            _x = x;
            _y = y;
            _width = Width;
            _height = Height;

            _cameraX = (_width + _x) / 2;
            _cameraY = (_height + _y) / 2;

            _blobs = new List<Blob>();
            _nameCache = new Dictionary<uint, string>();
        }

        public uint PlayerId
        {
            get
            {
                return _playerId;
            }
            set
            {
                _playerId = value;
            }
        }

        public double CameraX
        {
            get
            {
                return _cameraX;
            }
            set
            {
                _cameraX = value;
            }
        }

        public double CameraY
        {
            get
            {
                return _cameraY;
            }
            set
            {
                _cameraY = value;
            }
        }

        public List<Blob> Blobs
        {
            get
            {
                return _blobs;
            }
        }

        public override void Draw()
        {
            DrawGrid();
            DrawBorder();

            foreach (Blob b in _blobs.ToList())
            {
                if (b.x > _cameraX - b.Size && b.x < _cameraX + SwinGame.ScreenWidth() + b.Size)
                {
                    if (b.y > _cameraY - b.Size && b.y < _cameraY + SwinGame.ScreenHeight() + b.Size)
                    {
                        if (b.IsVirus)
                        {
                            Circle virus = new Circle(Color.Black, (int)(b.x - _cameraX), (int)(b.y - _cameraY), (int)b.Size + 3, false);
                            virus.Draw();
                        }
                        Circle circ = new Circle(SwinGame.RGBColor(b.r, b.g, b.b), (int)(b.x - _cameraX), (int)(b.y - _cameraY), (int)b.Size, false);
                        circ.Draw();

                        if (_nameCache.ContainsKey(b.Id))
                        {
                            SwinGame.DrawTextOnScreen(_nameCache[b.Id], Color.Black, (int)(b.x - _cameraX), (int)(b.y - _cameraY));
                        }
                        if (b.Size > 11)
                        {
                            SwinGame.DrawTextOnScreen(Math.Round(b.Size).ToString(), Color.Black, (int)(b.x - _cameraX), (int)(b.y - _cameraY + 10));
                        }
                    }
                }
            }
        }

        public override bool IsAt(Point2D pt)
        {
            throw new NotImplementedException();
        }

        public void DrawGrid()
        {
            for (int x = (int)((_cameraX / 6) % _gridSize); x < SwinGame.ScreenWidth(); x += _gridSize)
            {
                LineSegment seg = new LineSegment
                {
                    StartPoint = new Point2D { X = x, Y = 0 },
                    EndPoint = new Point2D { X = x, Y = SwinGame.ScreenHeight() }
                };

                SwinGame.DrawLine(Color.LightGray, seg);
            }

            for (int y = (int)((_cameraY / 6) % _gridSize); y < SwinGame.ScreenHeight(); y += _gridSize)
            {
                LineSegment seg = new LineSegment
                {
                    StartPoint = new Point2D { X = 0, Y = y },
                    EndPoint = new Point2D { X = SwinGame.ScreenWidth(), Y = y }
                };

                SwinGame.DrawLine(Color.LightGray, seg);
            }
        }

        public void DrawBorder()
        {
            //TOOD
        }

        public void Read(BinaryReader b)
        {
            Random rand = new Random();

            //Remove "little blobs" from the world
            int e = b.ReadUInt16();
            for (int i = 0; i < e; ++i)
            {
                var f = b.ReadUInt32();
                var g = b.ReadUInt32();

                foreach (Blob currBlob in _blobs.ToList())
                {
                    if (currBlob.Id == g || currBlob.Id == f)
                        _blobs.Remove(currBlob);
                }

                if (g == _playerId)
                {
                    if (PlayerDied != null)
                    {
                        PlayerDied();
                        _playerId = 0;
                    }
                }
            }

            //Add players
            while (true)
            {
                Blob blob = ReadBlob(b);
                if (blob == null)
                    break;

                bool AddBlob = true;
                foreach (Blob currBlob in _blobs.ToList())
                {
                    if (currBlob.Id == blob.Id)
                    {
                        AddBlob = false;
                        currBlob.x = blob.x;
                        currBlob.y = blob.y;
                    }
                }

                if (AddBlob)
                    _blobs.Add(blob);

                if (_playerId == blob.Id)
                {
                    _cameraX = blob.x - (SwinGame.ScreenWidth() / 2);
                    _cameraY = blob.y - (SwinGame.ScreenHeight() / 2);
                }
            }

            b.ReadUInt16();
            var test = b.ReadUInt32();
            uint test2 = 0;
            for (int i = 0; i < test; i++)
            {
                test2 = b.ReadUInt32();
            }
        }

        public Blob ReadBlob(BinaryReader b)
        {
            var Id = b.ReadUInt32();

            if (Id == 0)
                return null;

            var x = b.ReadDouble();
            var y = b.ReadDouble();
            var size = b.ReadDouble();

            b.ReadByte();

            var r = b.ReadByte();
            var g = b.ReadByte();
            var blue = b.ReadByte();

            bool IsVirus = b.ReadByte() == 0 ? false : true;
            string PlayerName = "";
            while (true)
            {
                var n = b.ReadUInt16();
                if (n == 0)
                    break;
                PlayerName += Convert.ToChar(n);
            }
            if (_nameCache.ContainsKey(Id) && PlayerName.Length > 0)
                _nameCache.Remove(Id);

            if (PlayerName.Length > 0)
                _nameCache.Add(Id, PlayerName);

            Blob newBlob = new Blob
            {
                Id = Id,
                ox = x,
                x = x,
                oy = y,
                y = y,
                Size = size,
                r = r,
                b = blue,
                g = g,
                IsVirus = IsVirus,
                Name = PlayerName
            };

            return newBlob;
        }
    }
}
