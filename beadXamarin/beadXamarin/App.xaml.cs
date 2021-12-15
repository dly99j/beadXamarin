using beadXamarin.Model;
using beadXamarin.Persistence;
using beadXamarin.View;
using beadXamarin.ViewModel;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace beadXamarin
{
    public partial class App : Application
    {
        #region Fields

        private IGameDataAccess _GameDataAccess;
        private GameModel _GameModel;
        private GameViewModel _GameViewModel;
        private GamePage _gamePage;
        private SettingsPage _settingsPage;

        private IStore _store;
        private StoredGameBrowserModel _storedGameBrowserModel;
        private StoredGameBrowserViewModel _storedGameBrowserViewModel;

        private Boolean _advanceTimer;
        private Boolean _updateTimer; //??
        private NavigationPage _mainPage;
        private Boolean _isPaused;
        private GameDifficulty _gameDifficulty;

        #endregion

        #region Application methods

        public App()
        {
            _GameDataAccess = DependencyService.Get<IGameDataAccess>(); // az interfész megvalósítását automatikusan megkeresi a rendszer

            _GameModel = new GameModel(_GameDataAccess);
            _GameModel.Over += new EventHandler<GameEventArgs>(GameModel_GameOver);

            _GameViewModel = new GameViewModel(_GameModel);
            _GameViewModel.NewGame += new EventHandler(GameViewModel_NewGame);
            _GameViewModel.ExitGame += new EventHandler(GameViewModel_ExitGame);

            _GameViewModel.KeyA += ViewModel_AKey;
            _GameViewModel.KeyD += ViewModel_DKey;
            _GameViewModel.KeyS += ViewModel_SKey;
            _GameViewModel.KeyW += ViewModel_WKey;

            _GameViewModel.PauseGame += GameViewModel_PauseGame;
            _GameViewModel.LevelOne += LevelOne;
            _GameViewModel.LevelTwo += LevelTwo;
            _GameViewModel.LevelThree += LevelThree;

            _gamePage = new GamePage();
            _gamePage.BindingContext = _GameViewModel;

            _settingsPage = new SettingsPage();
            _settingsPage.BindingContext = _GameViewModel;

            _store = DependencyService.Get<IStore>();
            _storedGameBrowserModel = new StoredGameBrowserModel(_store);
            _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
            _storedGameBrowserViewModel.GameLoading += new EventHandler<StoredGameEventArgs>(StoredGameBrowserViewModel_GameLoading);

            _mainPage = new NavigationPage(_gamePage);

            MainPage = _mainPage;
        }

        protected override void OnStart()
        {
            _GameModel.NewGame();
            _GameViewModel.RefreshTable();
            _advanceTimer = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; }); // elindítjuk az időzítőt
        }

        protected override void OnSleep()
        {
            _advanceTimer = false;

            try
            {
                Task.Run(async () => await _GameModel.SaveGameAsync("SuspendedGame"));
            }
            catch { }
        }

        protected override void OnResume()
        {
            try
            {
                Task.Run(async () =>
                {
                    await _GameModel.LoadGameAsync("SuspendedGame");
                    _GameViewModel.RefreshTable();

                    _advanceTimer = true;
                    Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; });
                });
            }
            catch { }

        }

        #endregion

        #region ViewModel event handlers

        private void ViewModel_WKey(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _GameModel.PlayerStep(GameDirection.Up);
                _GameModel.RefreshTable();
                _GameViewModel.RefreshTable();
            }
        }

        private void ViewModel_SKey(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _GameModel.PlayerStep(GameDirection.Down);
                _GameModel.RefreshTable();
                _GameViewModel.RefreshTable();
            }
        }

        private void ViewModel_DKey(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _GameModel.PlayerStep(GameDirection.Right);
                _GameModel.RefreshTable();
                _GameViewModel.RefreshTable();
            }
        }

        private void ViewModel_AKey(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _GameModel.PlayerStep(GameDirection.Left);
                _GameModel.RefreshTable();
                _GameViewModel.RefreshTable();
            }
        }

        private void GameViewModel_NewGame(object sender, EventArgs e)
        {
            _GameModel.NewGame();

            if (!_advanceTimer)
            {
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; });
            }
        }

        private async void ViewModel_StartGame(object sender, System.EventArgs e, GameDifficulty gameLevel, String fileName)
        {
            _gameDifficulty = gameLevel;

            try
            {
                await _GameModel.LoadGameAsync(fileName);
                _GameViewModel.CreateNewTable();
            }
            catch (GameDataException)
            {
            }

            if (!_advanceTimer)
            {
                _advanceTimer = true;
                _isPaused = false;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; });
            }
        }

        private void LevelOne(object sender, System.EventArgs e)
        {
            ViewModel_StartGame(sender, e, GameDifficulty.Easy, @"..\..\..\table1.txt");
        }

        private void LevelTwo(object sender, System.EventArgs e)
        {
            ViewModel_StartGame(sender, e, GameDifficulty.Medium, @"..\..\..\table2.txt");
        }

        private void LevelThree(object sender, System.EventArgs e)
        {
            ViewModel_StartGame(sender, e, GameDifficulty.Hard, @"..\..\..\table3.txt");
        }

        //private async void GameViewModel_LoadGame(object sender, System.EventArgs e)
        //{
        //    await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        //    await _mainPage.PushAsync(_loadGamePage); // átnavigálunk a lapra
        //}

        private async void ViewModel_ExitGame(object sender, System.EventArgs e)
        {
            await _mainPage.PushAsync(_settingsPage);
        }

        /// <summary>
        /// Játék mentésének eseménykezelője.
        /// </summary>
        private async void GameViewModel_SaveGame(object sender, EventArgs e)
        {
            await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
            //await _mainPage.PushAsync(_saveGamePage); // átnavigálunk a lapra
        }

        private async void GameViewModel_ExitGame(object sender, EventArgs e)
        {
            await _mainPage.PushAsync(_settingsPage); // átnavigálunk a beállítások lapra
        }

        private void GameViewModel_PauseGame(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _isPaused = true;

            }
            else if (_isPaused)
            {
                _isPaused = false;
            }
        }

        private void SneakingOutViewModel_RestartGame(object sender, EventArgs e)
        {
            if (_gameDifficulty == GameDifficulty.Easy)
            {
                LevelOne(sender, e);
            }
            if (_gameDifficulty == GameDifficulty.Medium)
            {
                LevelTwo(sender, e);
            }
            if (_gameDifficulty == GameDifficulty.Hard)
            {
                LevelThree(sender, e);
            }
        }

        /// <summary>
        /// Betöltés végrehajtásának eseménykezelője.
        /// </summary>
        private async void StoredGameBrowserViewModel_GameLoading(object sender, StoredGameEventArgs e)
        {
            await _mainPage.PopAsync(); // visszanavigálunk

            // betöltjük az elmentett játékot, amennyiben van
            try
            {
                await _GameModel.LoadGameAsync(e.Name);
                _GameViewModel.RefreshTable();

                // csak akkor indul az időzítő, ha sikerült betölteni a játékot
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; });
            }
            catch
            {
                await MainPage.DisplayAlert("Game játék", "Sikertelen betöltés.", "OK");
            }
        }

        #endregion

        #region Model event handlers

        private async void GameModel_GameOver(object sender, GameEventArgs e)
        {
            _advanceTimer = false;

            if (e.IsWon) // győzelemtől függő üzenet megjelenítése
            {
                await MainPage.DisplayAlert("MaciLaci játék", "Gratulálok, győztél!" + Environment.NewLine +
                                            "Összesen " +
                                            TimeSpan.FromSeconds(e.Time).ToString("g") + " ideig játszottál.",
                                            "OK");
            }
            else
            {
                await MainPage.DisplayAlert("Game játék", "Sajnálom, vesztettél, lejárt az idő!", "OK");
            }
        }

        #endregion
    }
}
