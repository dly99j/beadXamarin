namespace beadXamarin.ViewModel
{
    public class GameField : ViewModelBase
    {
        private bool _empty;
        private bool _food;
        private bool _guard;

        private bool _player;
        private string _text;
        private bool _tree;

        /// <summary>
        ///     Felirat lekérdezése, vagy beállítása.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                    setText();
                }
            }
        }

        /// <summary>
        ///     Vízszintes koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     Függőleges koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     Sorszám lekérdezése.
        /// </summary>
        public int Number { get; set; }

        public bool IsEmpty
        {
            get => _empty;
            set
            {
                if (_empty != value)
                {
                    _empty = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsGuard
        {
            get => _guard;
            set
            {
                if (_guard != value)
                {
                    _guard = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFood
        {
            get => _food;
            set
            {
                if (_food != value)
                {
                    _food = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlayer
        {
            get => _player;
            set
            {
                if (_player != value)
                {
                    _player = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTree
        {
            get => _tree;
            set
            {
                if (_tree != value)
                {
                    _tree = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        ///     Lépés parancs lekérdezése, vagy beállítása.
        /// </summary>
        public DelegateCommand StepCommand { get; set; }

        public void setFalse()
        {
            IsEmpty = false;
            IsPlayer = false;
            IsGuard = false;
            IsTree = false;
            IsFood = false;
        }

        public void setText()
        {
            setFalse();

            switch (_text)
            {
                case "E":
                    IsEmpty = true;
                    break;
                case "P":
                    IsPlayer = true;
                    break;
                case "T":
                    IsTree = true;
                    break;
                case "G":
                    IsGuard = true;
                    break;
                case "F":
                    IsFood = true;
                    break;
            }
        }
    }
}