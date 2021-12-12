using beadXamarin.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace beadXamarin.ViewModel
{

    public class GameViewModel : ViewModelBase
    {
        #region Fields

        private GameModel _model;
        private GameDifficulty _difficulty;

        #endregion

        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand LevelOneCommand { get; private set; }
        public DelegateCommand LevelTwoCommand { get; private set; }
        public DelegateCommand LevelThreeCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand WKeyCommand { get; private set; }
        public DelegateCommand AKeyCommand { get; private set; }
        public DelegateCommand SKeyCommand { get; private set; }
        public DelegateCommand DKeyCommand { get; private set; }
        public DelegateCommand PKeyCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SizeCommand { get; private set; }


        public ObservableCollection<GameField> Fields { get; set; }
        public int GameSize { get; set; }
        public String GameTime { get { return TimeSpan.FromSeconds(_model.Time).ToString("g"); } }
        public Boolean IsGameEasy
        {
            get { return _difficulty == GameDifficulty.Easy; }
            set
            {
                if (_difficulty == GameDifficulty.Easy)
                    return;

                _difficulty = GameDifficulty.Easy;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }
        public Boolean IsGameMedium
        {
            get { return _difficulty == GameDifficulty.Medium; }
            set
            {
                if (_difficulty == GameDifficulty.Medium)
                    return;

                _difficulty = GameDifficulty.Medium;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }
        public Boolean IsGameHard
        {
            get { return _difficulty == GameDifficulty.Hard; }
            set
            {
                if (_difficulty == GameDifficulty.Hard)
                    return;

                _difficulty = GameDifficulty.Hard;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }

        #endregion

        #region Events

        public event EventHandler NewGame;
        public event EventHandler LoadGame;
        public event EventHandler ExitGame;
        public event EventHandler KeyW;
        public event EventHandler KeyA;
        public event EventHandler KeyS;
        public event EventHandler KeyD;
        public event EventHandler KeyP;
        public event EventHandler PauseGame;
        public event EventHandler LevelOne;
        public event EventHandler LevelTwo;
        public event EventHandler LevelThree;
        public event EventHandler SizeEvent;

        #endregion

        #region Constructors

        public GameViewModel(GameModel model)
        {
            _model = model;
            _model.Advanced += new EventHandler<GameEventArgs>(Model_GameAdvanced);
            _model.Over += new EventHandler<GameEventArgs>(Model_GameOver);
            _model.Refresh += new EventHandler<GameEventArgs>(Model_GameCreated);
            _model.Created += new EventHandler<GameEventArgs>(Model_GameCreated);


            PauseCommand = new DelegateCommand(param => OnPauseGame());
            LevelOneCommand = new DelegateCommand(param => OnLevelOne());
            LevelTwoCommand = new DelegateCommand(param => OnLevelTwo());
            LevelThreeCommand = new DelegateCommand(param => OnLevelThree());
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            WKeyCommand = new DelegateCommand(param => OnKeyW());
            AKeyCommand = new DelegateCommand(param => OnKeyA());
            SKeyCommand = new DelegateCommand(param => OnKeyS());
            DKeyCommand = new DelegateCommand(param => OnKeyD());
            PKeyCommand = new DelegateCommand(param => OnKeyP());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SizeCommand = new DelegateCommand(param => OnSize());

            CreateNewTable();

            RefreshTable();
        }

        #endregion

        #region Private methods

        public void CreateNewTable()
        {
            Fields = new ObservableCollection<GameField>();
            GameSize = _model.Height;
            for (Int32 i = 0; i < _model.Height; i++)
            {
                for (Int32 j = 0; j < _model.Width; j++)
                {
                    Fields.Add(new GameField
                    {
                        Text = "",
                        X = i,
                        Y = j,
                        Number = i * _model.Width + j,
                    });

                    //OnPropertyChanged("Fields");
                }
            }

            RefreshTable();
        }

        private void Logger(string msg)
        {
            if (!File.Exists(@"log.txt"))
            {
                File.CreateText(@"log.txt");
            }
            var tw = File.AppendText(@"log.txt");
            tw.WriteLine(msg);
            tw.Close();
        }

        private void SyncGameTable(ObservableCollection<GameField> Fields)
        {
            for (Int32 i = 0; i < _model.Height; i++)
            {
                for (Int32 j = 0; j < _model.Width; j++)
                {
                    Fields[i * _model.Height + j].X = i;
                    Fields[i * _model.Height + j].Y = j;

                    Fields[i * _model.Height + j].Text = _model.CharTableRepresentation[i, j].ToString();
                    Fields[i * _model.Height + j].setText();
                    //OnPropertyChanged("Fields");
                }
            }
        }

        public void RefreshTable()
        {
            GameSize = _model.Height;
            foreach (var field in Fields) // inicializálni kell a mezőket is
            {
                field.Text = _model.CharTableRepresentation[field.X, field.Y].ToString();
                field.setFalse();
                SyncGameTable(Fields);
            }
            OnPropertyChanged("Fields");
            OnPropertyChanged("GameTime");
        }

        /// <summary>
        /// Játék léptetése eseménykiváltása.
        /// </summary>
        /// <param name="index">A lépett mező indexe.</param>
        private void StepGame(Int32 index)
        {
            GameField field = Fields[index];

            field.Text = _model.CharTableRepresentation[field.X, field.Y].ToString();
            RefreshTable();
        }

        #endregion

        #region Game event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void Model_GameOver(object sender, GameEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Játék előrehaladásának eseménykezelője.
        /// </summary>
        private void Model_GameAdvanced(object sender, GameEventArgs e)
        {
            OnPropertyChanged("GameTime");
            //OnPropertyChanged("Fields");
            RefreshTable();
        }

        /// <summary>
        /// Játék létrehozásának eseménykezelője.
        /// </summary>
        private void Model_GameCreated(object sender, GameEventArgs e)
        {
            CreateNewTable();
            RefreshTable();
        }

        #endregion

        #region Event methods

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnPauseGame()
        {
            PauseGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnLevelOne()
        {
            LevelOne?.Invoke(this, EventArgs.Empty);
        }
        private void OnLevelTwo()
        {
            LevelTwo?.Invoke(this, EventArgs.Empty);
        }
        private void OnLevelThree()
        {
            LevelThree?.Invoke(this, EventArgs.Empty);
        }
        private void OnKeyW()
        {
            KeyW?.Invoke(this, EventArgs.Empty);
        }
        private void OnKeyA()
        {
            KeyA?.Invoke(this, EventArgs.Empty);
        }
        private void OnKeyS()
        {
            KeyS?.Invoke(this, EventArgs.Empty);
        }
        private void OnKeyD()
        {
            KeyD?.Invoke(this, EventArgs.Empty);
        }
        private void OnKeyP()
        {
            KeyP?.Invoke(this, EventArgs.Empty);
        }
        private void OnSize()
        {
            SizeEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
