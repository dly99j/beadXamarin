using System;
using System.Collections.ObjectModel;
using beadXamarin.Model;

namespace beadXamarin.ViewModel
{
    /// <summary>
    /// Tárolt játékkezelő nézetmodellje.
    /// </summary>
    public class StoredGameBrowserViewModel : ViewModelBase
    {
        private StoredGameBrowserModel _model;

        /// <summary>
        /// Betöltés eseménye.
        /// </summary>
        public event EventHandler<StoredGameEventArgs> GameLoading;
        

        /// <summary>
        /// Tárolt játékkezelő nézetmodelljének példányosítása.
        /// </summary>
        /// <param name="model">A modell.</param>
        public StoredGameBrowserViewModel(StoredGameBrowserModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _model = model;
            _model.StoreChanged += new EventHandler(Model_StoreChanged);

            StoredGames = new ObservableCollection<StoredGameViewModel>();
            UpdateStoredGames();
        }

        /// <summary>
        /// Új jtáék parancsa.
        /// </summary>
        public DelegateCommand NewSaveCommand { get; private set; }

        /// <summary>
        /// Tárolt játékok gyűjteménye.
        /// </summary>
        public ObservableCollection<StoredGameViewModel> StoredGames { get; private set; }

        /// <summary>
        /// Tárolt játékok frissítése.
        /// </summary>
        private void UpdateStoredGames()
        {
            StoredGames.Clear();

            foreach (StoredGameModel item in _model.StoredGames)
            {
                StoredGames.Add(new StoredGameViewModel
                {
                    Name = item.Name,
                    Modified = item.Modified,
                    LoadGameCommand = new DelegateCommand(param => OnGameLoading((String)param)),
                });
            }
        }

        private void Model_StoreChanged(object sender, EventArgs e)
        {
            UpdateStoredGames();
        }

        private void OnGameLoading(String name)
        {
            if (GameLoading != null)
                GameLoading(this, new StoredGameEventArgs { Name = name });
        }

    }
}
