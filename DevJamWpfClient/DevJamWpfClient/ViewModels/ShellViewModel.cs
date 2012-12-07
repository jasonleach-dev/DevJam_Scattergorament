using Caliburn.Micro;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DevJamWpfClient.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        private HubConnection _hubConnection;
        private IHubProxy _gameHub;
        
        private DispatcherTimer _timer;

        public ShellViewModel()
        {
            _hubConnection = new HubConnection("http://localhost:52029/");
            _gameHub = _hubConnection.CreateHubProxy("ScattergoramentHub");

            _gameHub.On<char, DateTime>("gameStart", GameStart);
            _gameHub.On<string, DateTime>("gameEnd", GameEnd);
            _gameHub.On<bool, char, DateTime>("gameStatus", GameStatus);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _hubConnection.Start()
                .ContinueWith(task=>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("An error occurred during the method call {0}", task.Exception.GetBaseException());
                        }
                        else
                        {
                            RegisterPlayer();
                        }
                    }
                    );
        }

        #region Game Code

        void _timer_Tick(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => TimeToEnd);
            NotifyOfPropertyChange(() => TimeToStart);
        }

        private string _guess;
        public string Guess {
            get { return _guess; }
            set
            {
                _guess = value;
                NotifyOfPropertyChange(() => Guess);
            }
        }
        
        private DateTime _gameEndTime;
        public DateTime GameEndTime
        {
            get { return _gameEndTime; }
            set
            {
                _gameEndTime = value;
                NotifyOfPropertyChange(() => GameEndTime);
                NotifyOfPropertyChange(() => TimeToEnd);
            }
        }

        public int TimeToStart
        {
            get
            {
                return (int)(GameStartTime - DateTime.Now).TotalSeconds + 1;
            }
        }

        public int TimeToEnd
        {
            get
            {
                return (int)(GameEndTime - DateTime.Now).TotalSeconds + 1;
            }
        }
        
        private DateTime _gameStartTime;
        public DateTime GameStartTime
        {
            get { return _gameStartTime; }
            set
            {
                _gameStartTime = value;
                NotifyOfPropertyChange(() => GameStartTime);
                NotifyOfPropertyChange(() => TimeToStart);
            }
        }
        
        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                NotifyOfPropertyChange(() => IsRunning);
                NotifyOfPropertyChange(() => ShowGame);
                NotifyOfPropertyChange(() => ShowWait);
            }
        }

        public bool ShowGame
        {
            get{
                return _isRunning;
            }
        }

        public bool ShowWait
        {
            get
            {
                return !_isRunning;
            }
        }

        private char _hint;
        public char Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                NotifyOfPropertyChange(() => Hint);

            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        #endregion

        #region Calls from server

        private void GameStart(char letter, DateTime approxEnd)
        {
            IsRunning = true;
            GameEndTime = approxEnd;
            Hint = letter;
            Message = "";
        }

        private void GameEnd(string message, DateTime approxStart)
        {
            IsRunning = false;
            GameStartTime = approxStart;
            Message = message;
        }

        private void GameStatus(bool isRunning, char hint, DateTime approxNextState)
        {
            IsRunning = isRunning;
            if (isRunning)
            {
                GameEndTime = approxNextState;
                Hint = hint;
            }
            else
            {
                GameStartTime = approxNextState;

            }
        }

        #endregion

        #region Calls to Server

        public void MakeGuess()
        {
            _gameHub.Invoke("SetGuess", Guess).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("An error occurred during the method call {0}", task.Exception.GetBaseException());
                }
                else
                {
                    Message = Message + System.Environment.NewLine + "Guess submitted";
                }
            });

        }

        private void RegisterPlayer()
        {
            _gameHub.Invoke("RegisterPlayer").ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("An error occurred during the method call {0}", task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Successfully called MethodOnServer");
                }
            });

        }

        #endregion
    }
}
