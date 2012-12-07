using DevJamServer.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace DevJamServer.Worker
{
    public class Scattergorament
    {
        #region Game Code - Yes regions, sue me :)

        private enum GameState
        {
            Waiting,
            InProgress
        }

        private readonly TimeSpan _gameTime = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _delayTime = TimeSpan.FromSeconds(5);

        private GameState _state = GameState.Waiting;
        private DateTime _nextStateTime;
        private System.Timers.Timer _gameTimer = new System.Timers.Timer();

        private Dictionary<string, Player> _players = new Dictionary<string,Player>();
        private char _gameLetter = 'a';
        private string _gameWord = "Albatross";

        private static Scattergorament _instance;
        private static object _lock = new object();

        public static Scattergorament Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Scattergorament();
                    }
                }
                return _instance;
            }
        }

        private Scattergorament()
        {
            _gameTimer.AutoReset = false;
            _gameTimer.Elapsed += _gameTimer_Elapsed;
        }

        void _gameTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_state == GameState.Waiting)
            {
                //start game - set up word
                _gameLetter = _gameWord.Substring(0, 1).First();

                //clear and reset
                foreach (var player in _players)
                {
                    player.Value.Guess = string.Empty;
                }
                //broadcast to players
                DateTime approxEnd = DateTime.Now.AddSeconds(_gameTime.TotalSeconds);
                Debug.WriteLine("Game Starting - GameWord: {0} | ApproxEndTime: {1:T}", _gameWord, approxEnd);
                SendGameStart(_gameLetter, approxEnd);

                _state = GameState.InProgress;
                _gameTimer.Interval = _gameTime.TotalMilliseconds;
                _nextStateTime = DateTime.Now + _gameTime;
            }
            else
            {
                //end game
                _state = GameState.Waiting;

                var sb = new StringBuilder();

                foreach (var player in _players)
                {
                    sb.AppendLine(string.Format("{0} guessed {1}", player.Value.Id, player.Value.Guess));
                }

                sb.AppendLine(string.Format("The winner was: {0}", "No one, you are all terrible players"));
                //broadcast to players
                DateTime approxStart = DateTime.Now.AddSeconds(_delayTime.TotalSeconds);
                SendGameEnd(sb.ToString(), approxStart);
                
                _gameTimer.Interval = _delayTime.TotalMilliseconds;
                _nextStateTime = DateTime.Now + _delayTime;
            }

            _gameTimer.Start();
        }

        public void Start()
        {
            _state = GameState.Waiting;
            _gameTimer.Interval = _delayTime.TotalMilliseconds;
            _gameTimer.Start();
        }

        #endregion

        #region Called from clients

        public void AddPlayer(string playerId)
        {
            lock (_lock)
            {
                _players.Add(playerId, new Player() { Id = playerId });
            }
        }

        public void SetGuess(string playerId, string guess)
        {
            lock (_lock)
            {
                _players[playerId].Guess = guess;
            }
        }

        #endregion

        #region Calls to clients

        public void SendStatusUpdate(dynamic caller)
        {
            caller.GameStatus(_state == GameState.InProgress, _gameLetter, _nextStateTime);
        }

        private void SendGameStart(char letter, DateTime approxEnd)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ScattergoramentHub>();
            hub.Clients.All.gameStart(_gameLetter, approxEnd);
        }

        private void SendGameEnd(string message, DateTime approxStart)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ScattergoramentHub>();
            hub.Clients.All.gameEnd(message, approxStart);
        }

        #endregion
    }
}