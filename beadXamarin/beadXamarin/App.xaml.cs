using System;
using System.Threading.Tasks;
using beadXamarin.Model;
using beadXamarin.Persistence;
using beadXamarin.View;
using beadXamarin.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Game
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
        private LoadGamePage _loadGamePage;

        private Boolean _advanceTimer;
        private NavigationPage _mainPage;

        #endregion

        #region Application methods

        public App()
        {
            // játék összeállítása
            _GameDataAccess = DependencyService.Get<IGameDataAccess>(); // az interfész megvalósítását automatikusan megkeresi a rendszer

            _GameModel = new GameModel(_GameDataAccess);
            _GameModel.Over += new EventHandler<GameEventArgs>(GameGameModel_GameOver);

            _GameViewModel = new GameViewModel(_GameModel);
            _GameViewModel.NewGame += new EventHandler(GameViewModel_NewGame);
            _GameViewModel.LoadGame += new EventHandler(GameViewModel_LoadGame);
            _GameViewModel.ExitGame += new EventHandler(GameViewModel_ExitGame);

            _gamePage = new GamePage();
            _gamePage.BindingContext = _GameViewModel;

            _settingsPage = new SettingsPage();
            _settingsPage.BindingContext = _GameViewModel;

            // a játékmentések kezelésének összeállítása
            _store = DependencyService.Get<IStore>(); // a perzisztencia betöltése az adott platformon
            _storedGameBrowserModel = new StoredGameBrowserModel(_store);
            _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
            _storedGameBrowserViewModel.GameLoading += new EventHandler<StoredGameEventArgs>(StoredGameBrowserViewModel_GameLoading);

            _loadGamePage = new LoadGamePage();
            _loadGamePage.BindingContext = _storedGameBrowserViewModel;

            // nézet beállítása
            _mainPage = new NavigationPage(_gamePage); // egy navigációs lapot használunk fel a három nézet kezelésére

            MainPage = _mainPage;
        }

        protected override void OnStart()
        {
            _GameModel.NewGame();
            _GameViewModel.RefreshTable();
            _advanceTimer = true; // egy logikai értékkel szabályozzuk az időzítőt
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; }); // elindítjuk az időzítőt
        }

        protected override void OnSleep()
        {
            _advanceTimer = false;

            //// elmentjük a jelenleg folyó játékot
            //try
            //{
            //    Task.Run(async () => await _GameModel.SaveGameAsync("SuspendedGame"));
            //}
            //catch { }
        }

        //protected override void OnResume()
        //{
        //    // betöltjük a felfüggesztett játékot, amennyiben van
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            await _GameGameModel.LoadGameAsync("SuspendedGame");
        //            _GameViewModel.RefreshTable();

        //            // csak akkor indul az időzítő, ha sikerült betölteni a játékot
        //            _advanceTimer = true;
        //            Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameGameModel.AdvanceTime(); return _advanceTimer; });
        //        });
        //    }
        //    catch { }

        //}

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// Új játék indításának eseménykezelője.
        /// </summary>
        private void GameViewModel_NewGame(object sender, EventArgs e)
        {
            _GameModel.NewGame();

            if (!_advanceTimer)
            {
                // ha nem fut az időzítő, akkor elindítjuk
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _GameModel.AdvanceTime(); return _advanceTimer; });
            }
        }

        /// <summary>
        /// Játék betöltésének eseménykezelője.
        /// </summary>
        private async void GameViewModel_LoadGame(object sender, System.EventArgs e)
        {
            await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
            await _mainPage.PushAsync(_loadGamePage); // átnavigálunk a lapra
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

        /// <summary>
        /// Mentés végrehajtásának eseménykezelője.
        /// </summary>
        //private async void StoredGameBrowserViewModel_GameSaving(object sender, StoredGameEventArgs e)
        //{
        //    await _mainPage.PopAsync(); // visszanavigálunk
        //    _advanceTimer = false;

        //    try
        //    {
        //        // elmentjük a játékot
        //        await _GameGameModel.SaveGameAsync(e.Name);
        //    }
        //    catch { }

        //    await MainPage.DisplayAlert("Game játék", "Sikeres mentés.", "OK");
        //}

        #endregion

        #region Model event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private async void GameGameModel_GameOver(object sender, GameEventArgs e)
        {
            _advanceTimer = false;

            if (e.IsWon) // győzelemtől függő üzenet megjelenítése
            {
                await MainPage.DisplayAlert("Game játék", "Gratulálok, győztél!" + Environment.NewLine +
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
