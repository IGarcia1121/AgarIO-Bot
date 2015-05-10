using System;
using System.Linq;
using SwinGameSDK;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.IO;

namespace Petri
{
    public class GameMain
    {
        private static List<Server> _serverList;
        private static Dictionary<int, int> _packetCount;
        private static bool _startup;
        private static string _packetText;
        private static string _ipAddress;

        private static List<LeaderboardPlayer> _leaderboard;
        private static int _leaderboardMarquee;

        private static ComboBox _box;
        private static bool _showPopup;

        private static WebSocketWrapper _ws;
        private static bool _connected;
        private static World _agarWorld;

        public static void Main()
        {
            //Create collections to hold leaderboard, packet count and server list
            _leaderboard = new List<LeaderboardPlayer>();
            _packetCount = new Dictionary<int, int>();
            _serverList = new List<Server>();

            _showPopup = true;

            //Open the game window
            SwinGame.OpenGraphicsWindow("Petri", 1024, 800);

            //Load the Arial font
            Font _gameFont = SwinGame.LoadFont("Arial", 14);

            Rectangle background = new Rectangle(SwinGame.RGBAColor(0, 0, 0, 155), (SwinGame.ScreenWidth() / 2) - (300 / 2), 
                                                                                   (SwinGame.ScreenHeight() / 2) - (200 / 2), 300, 150, false);

            //Create a textbox to get the nick name
            TextBox nameEntry = new TextBox(_gameFont, (SwinGame.ScreenWidth() / 2) - (190 / 2), background.Y + 30, 190, 20, 16);

            //Create a combobox to select the server 
            _box = new ComboBox(_gameFont, nameEntry.X, background.Y + 70, 190, 20);
            _box.ItemSelected += box_ItemSelected;
            _box.Text = "Loading servers...";

            Rectangle connectRect = new Rectangle(Color.DarkGray, _box.X, background.Y + background.Height - 30, 190, 20, false);

            while (!SwinGame.WindowCloseRequested())
            {
                //Init the server info on load
                if (!_startup)
                {
                    _startup = true;
                    LoadData(_box);
                }

                //Check for any clicks/key presses
                SwinGame.ProcessEvents();

                //Clear the screen
                SwinGame.ClearScreen(Color.White);

                //Draw the world and contents within
                if (_agarWorld != null)
                    _agarWorld.Draw();

                //Draw menu
                if (_showPopup)
                {
                    background.Draw();
                    
                    SwinGame.DrawTextOnScreen("Name:", Color.White, nameEntry.X, nameEntry.Y - 10);
                    SwinGame.DrawTextOnScreen("Server:", Color.White, _box.X, _box.Y - 10);
                    nameEntry.Draw();

                    if (_connected)
                    {
                        connectRect.Draw();
                        SwinGame.DrawTextOnScreen("Play", Color.White, connectRect.X + (connectRect.Width / 2) - 16, connectRect.Y + 6);

                        if (SwinGame.MouseClicked(MouseButton.LeftButton) && connectRect.IsAt(SwinGame.MousePosition()) && !_box.DroppedDown)
                        {
                            Play(nameEntry.Text);
                        }
                    }

                    _box.Draw();
                }
                else
                {
                    if (SwinGame.MouseDown(MouseButton.LeftButton))
                    {
                        Movement();
                        //PushMessage(17);
                    }
                }



                //Draw server info
                SwinGame.DrawTextOnScreen(string.Format("{0} FPS", SwinGame.GetFramerate()), Color.Black, 0, 0);

                if (_agarWorld != null)
                    SwinGame.DrawTextOnScreen(string.Format("{0},{1}", Math.Round(_agarWorld.CameraX), Math.Round(_agarWorld.CameraY)), Color.Black, 0, 10);

                SwinGame.DrawTextOnScreen(_packetText, Color.Black, 0, 20);
                SwinGame.DrawTextOnScreen(_ipAddress, Color.Black, 0, 30);

                //Draw Leaderboard Info
                string players = "";
                foreach (LeaderboardPlayer p in _leaderboard)
                {
                    players += string.Format("{0}. {1} ", p.Rank, p.Name);
                }
                SwinGame.DrawTextOnScreen(players, Color.Black, _leaderboardMarquee, SwinGame.ScreenHeight() - 10);
                if (players.Length * 8 > SwinGame.ScreenWidth())
                {
                    _leaderboardMarquee += 1;
                }
                else
                {
                    _leaderboardMarquee = 0;
                }
                if (_leaderboardMarquee > SwinGame.ScreenWidth())
                {
                    _leaderboardMarquee = -(players.Length * 8);
                }



                //Refresh the screen at ~60 fps
                SwinGame.RefreshScreen(60);
            }
        }

        /// <summary>
        /// Log a packet into the packet count
        /// </summary>
        /// <param name="id">The id of the packet</param>
        public static void RecordPacket(byte id)
        {
            if (!_packetCount.ContainsKey(id))
                _packetCount[id] = 0;
            _packetCount[id] += 1;

            string result = "Packet count: ";
            foreach (KeyValuePair<int, int> kvPair in _packetCount.ToList())
            {
                result += string.Format("{0} ~ {1} | ", kvPair.Key, kvPair.Value);
            }
            _packetText = result;
        }

        /// <summary>
        /// Load the server data into the combo box
        /// </summary>
        /// <param name="box">The combo box to load the servers into</param>
        public async static void LoadData(ComboBox box)
        {
            using (var client = new HttpClient())
            {
                var serverJSON = await client.GetStringAsync("http://m.agar.io/info");

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var servers = serializer.Deserialize<Dictionary<string, object>>(serverJSON);

                foreach (var server in (Dictionary<string, object>)servers["regions"])
                {
                    Server newServer = new Server();
                    newServer.Name = server.Key;

                    var serverData = (Dictionary<string, object>)server.Value;
                    newServer.Players = (int)serverData["numPlayers"];
                    newServer.Realms = (int)serverData["numRealms"];
                    newServer.Servers = (int)serverData["numServers"];
                    _serverList.Add(newServer);

                    box.AddItem(newServer.Name);
                }

                box.Text = "Select a server";
            }
        }

        public static async void box_ItemSelected(string Item)
        {
            Server selectedServer = null;
            foreach (Server s in _serverList)
            {
                if (s.Name == Item)
                {
                    selectedServer = s;
                }
            }

            if (selectedServer == null)
                return;

            string ServerURL = "";
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string> { { selectedServer.Name, "" } };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("http://m.agar.io/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                ServerURL = responseString.Split('\n')[0];
            }

            ConnectToServer(ServerURL);
        }

        public static void ConnectToServer(string IP)
        {
            if (_ws != null)
            {
                DisposeWorld();
            }

            _ipAddress = string.Format("ws://{0}", IP);
            Uri serverURI = new Uri(string.Format("ws://{0}", IP));
            _ws = new WebSocketWrapper(string.Format("ws://{0}", IP));
            _ws.OnConnect(Connect);
            _ws.OnMessage(Message);
            _ws.OnDisconnect(Disconnect);

            _ws.Connect();
        }

        public static void DisposeWorld()
        {
            _connected = false;

            if (_ws != null)
                _ws.Close();

            _ws = null;

            if (_agarWorld != null)
                _agarWorld.PlayerDied -= _agarWorld_PlayerDied;
            _agarWorld = null;
            _showPopup = true;
            _ipAddress = "";
            _leaderboard = new List<LeaderboardPlayer>();
        }

        public static void Disconnect (WebSocketWrapper wrapper)
        {
            DisposeWorld();
            _box.Text = "Select a server";
        }

        public static void Connect(WebSocketWrapper wrapper)
        {
            byte[] handshake = new byte[5];
            handshake[0] = 255;
            handshake.SetUInt32(1, 1);

            wrapper.SendMessage(handshake);

            _connected = true;
        }

        public static void Play(string Name)
        {
            if (_ws == null)
                return;

            byte[] nickPacket = new byte[1 + (2 * Name.Length)];
            nickPacket[0] = 0;
            for (int i = 0; i < Name.Length; ++i )
            {
                nickPacket.SetUInt16(1 + 2 * i, Convert.ToUInt16(Name[i]));
            }

            _ws.SendMessage(nickPacket);
        }

        public static void Movement()
        {
            if (_ws == null)
                return;

            byte[] signalPacket = new byte[21];
            signalPacket[0] = 16;

            Point2D MouseLocation = SwinGame.MousePosition();
            double PointX = MouseLocation.X ;
            double PointY = MouseLocation.Y ;
            PointX += _agarWorld.CameraX;
            PointY += _agarWorld.CameraY;

            signalPacket.SetFloat(1, PointX);
            signalPacket.SetFloat(9, PointY);
            signalPacket.SetUInt32(17, 0);

            _ws.SendMessage(signalPacket);
        }

        public static void PushMessage(byte id)
        {
            if (_ws == null)
                return;

            byte[] messagePacket = new byte[1];
            messagePacket[0] = id;

            _ws.SendMessage(messagePacket);
        }

        public static void Message(byte[] s, WebSocketWrapper wrapper)
        {
            using (BinaryReader b = new BinaryReader(new MemoryStream(s)))
            {
                byte id = b.ReadByte();

                RecordPacket(id);

                if (id == 16) //Player Packet
                {
                    if (_agarWorld != null)
                    {
                        _agarWorld.Read(b);
                    }
                }
                else if (id == 32) //Player entry packet
                {
                    _agarWorld.PlayerId = b.ReadUInt32();
                    _showPopup = false;
                }
                else if (id == 49) //Leaderboard List
                {
                    _leaderboard = ReadLeaderboard(b);
                }
                else if (id == 64) //World size packet
                {
                    _agarWorld = new World(b.ReadDouble(), b.ReadDouble(), b.ReadDouble(), b.ReadDouble());
                    _agarWorld.PlayerDied += _agarWorld_PlayerDied;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        static void _agarWorld_PlayerDied()
        {
            _showPopup = true;
        }

        public static List<LeaderboardPlayer> ReadLeaderboard(BinaryReader b)
        {
            List<LeaderboardPlayer> players = new List<LeaderboardPlayer>();

            uint playerCount = b.ReadUInt32();
            for (int i = 0; i < playerCount; ++i)
            {
                uint playerId = b.ReadUInt32();
                string playerName = "";
                while (true)
                {
                    char character = (char)b.ReadUInt16();
                    if (character == 0)
                        break;
                    playerName += character;
                }

                players.Add(new LeaderboardPlayer { Id = playerId, Name = playerName, Rank = i + 1 });
            }

            return players;
        }
    }
}