using beadXamarin.Persistence;
using System;
using System.Threading.Tasks;

namespace beadXamarin.Model
{
    public enum GameDifficulty { Easy, Medium, Hard }

    public class GameModel
    {
        #region constructors

        public GameModel(IGameDataAccess dataAccess)
        {
            mDataAccess = dataAccess;
            mGameTable = new GameTable();
        }

        #endregion

        #region data

        private readonly IGameDataAccess mDataAccess;
        private GameDifficulty _gameDifficulty;

        #endregion

        #region properties


        public int FoodCount { get; private set; }

        public int Time { get; private set; }

        public GameTable mGameTable { get; private set; }

        public bool IsGameOver => mGameTable.Ended;

        public int Width => mGameTable.N;
        public int Height => mGameTable.M;
        public char[,] CharTableRepresentation => mGameTable.TableCharRepresentation;
        public GameDifficulty GameDifficulty { get { return _gameDifficulty; } set { _gameDifficulty = value; } }

        #endregion

        #region private methods

        private GameDirection OppositeDir(GameDirection dir)
        {
            switch (dir)
            {
                case GameDirection.Up:
                    return GameDirection.Down;
                case GameDirection.Right:
                    return GameDirection.Left;
                case GameDirection.Down:
                    return GameDirection.Up;
                case GameDirection.Left:
                    return GameDirection.Right;
            }

            throw new Exception("did I add an extra to the enum? or idk, check this. i just want to stop the " +
                                "compiler from giving me an error that this might not return");
        }

        private bool CanMove(GameObject movable, GameDirection dir)
        {
            var posM = movable.Position.Item1;
            var posN = movable.Position.Item2;
            switch (dir)
            {
                case GameDirection.Up:
                    if (posM - 1 >= 0 &&
                        mGameTable.TableCharRepresentation[posM - 1, posN] != 'T')
                        return true;
                    break;
                case GameDirection.Left:
                    if (posN - 1 >= 0 &&
                        mGameTable.TableCharRepresentation[posM, posN - 1] != 'T')
                        return true;
                    break;
                case GameDirection.Down:
                    if (posM + 1 < Height &&
                        mGameTable.TableCharRepresentation[posM + 1, posN] != 'T')
                        return true;
                    break;
                case GameDirection.Right:
                    if (posN + 1 < Width &&
                        mGameTable.TableCharRepresentation[posM, posN + 1] != 'T')
                        return true;
                    break;
            }

            return false;
        }

        private void MoveMovable(GameObject movable, GameDirection dir)
        {
            var posM = movable.Position.Item1;
            var posN = movable.Position.Item2;
            if (CanMove(movable, dir))
                switch (dir)
                {
                    case GameDirection.Up:
                        movable.Position = new Tuple<int, int>(posM - 1, posN);
                        break;
                    case GameDirection.Left:
                        movable.Position = new Tuple<int, int>(posM, posN - 1);
                        break;
                    case GameDirection.Down:
                        movable.Position = new Tuple<int, int>(posM + 1, posN);
                        break;
                    case GameDirection.Right:
                        movable.Position = new Tuple<int, int>(posM, posN + 1);
                        break;
                }
        }

        private void MoveGuard(GameGuard g)
        {
            if (!CanMove(g, g.Direction))
                g.Direction = OppositeDir(g.Direction);

            MoveMovable(g, g.Direction);
        }

        private bool IsVisibleForGuards()
        {
            var playerM = mGameTable.Player.Position.Item1;
            var playerN = mGameTable.Player.Position.Item2;
            foreach (var v in mGameTable.Guards)
            {
                var guardM = v.Position.Item1;
                var guardN = v.Position.Item2;

                if ((guardM - 1 <= playerM && playerM <= guardM + 1) &&
                    (guardN - 1 <= playerN && playerN <= guardN + 1))
                    return true;
            }

            return false;
        }

        #endregion

        #region public methods

        public void NewGame()
        {
            mGameTable = new GameTable();

            OnGameCreated();
        }

        public async Task MoveGuards()
        {
            foreach (var g in mGameTable.Guards) MoveGuard(g);
        }

        public async Task MovePlayer(GameDirection dir)
        {
            if (CanMove(mGameTable.Player, dir)) MoveMovable(mGameTable.Player, dir);

            foreach (var v in mGameTable.Foods)
                if (v.Position.Equals(mGameTable.Player.Position))
                {
                    mGameTable.Foods.Remove(v);
                    --FoodCount;
                    break;
                }
        }

        public void UpdateTable()
        {
            var newTable = new char[Height, Width];

            for (var i = 0; i < Height; ++i)
                for (var j = 0; j < Width; ++j)
                    newTable[i, j] = 'E';

            foreach (var v in mGameTable.Foods)
                newTable[v.Position.Item1, v.Position.Item2] = 'F';

            foreach (var v in mGameTable.Trees)
                newTable[v.Position.Item1, v.Position.Item2] = 'T';

            foreach (var v in mGameTable.Guards)
                newTable[v.Position.Item1, v.Position.Item2] = 'G';

            newTable[mGameTable.Player.Position.Item1, mGameTable.Player.Position.Item2] = 'P';

            mGameTable.TableCharRepresentation = newTable;
        }

        public async Task LoadGameAsync(string path)
        {
            if (mDataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            mGameTable = await mDataAccess.LoadAsync(path);
            Time = 0;
            FoodCount = mGameTable.NumOfFood;
        }

        public async Task SaveGameAsync(String path)
        {
            if (mDataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await mDataAccess.SaveAsync(path, mGameTable);
        }

        public void GameOver()
        {
            if (FoodCount == 0)
                Over?.Invoke(this, new GameEventArgs(Time, true, FoodCount, mGameTable));
            else
                Over?.Invoke(this, new GameEventArgs(Time, false, FoodCount, mGameTable));
        }

        public async void GameStep()
        {
            if (IsGameOver)
                return;

            await MoveGuards();
            UpdateTable();

            if (IsVisibleForGuards() || FoodCount == 0)
            {
                mGameTable.Ended = true;
                GameOver();
            }
        }

        public async void PlayerStep(GameDirection dir)
        {
            if (IsGameOver)
                return;

            await MovePlayer(dir);
            UpdateTable();
            PlayerMove?.Invoke(this, new GameEventArgs(Time, IsGameOver, FoodCount, mGameTable));

            if (IsVisibleForGuards() || FoodCount == 0)
            {
                mGameTable.Ended = true;
                GameOver();
            }
        }
        public void AdvanceTime()
        {
            if (IsGameOver)
                return;

            ++Time;
            Advanced?.Invoke(this, new GameEventArgs(Time, false, FoodCount, mGameTable));

            GameStep();
        }

        public void RefreshTable()
        {
            if (IsGameOver)
                return;

            Refresh?.Invoke(this, new GameEventArgs(Time, false, FoodCount, mGameTable));
        }

        public void OnGameCreated()
        {
            Created?.Invoke(this, new GameEventArgs(Time, false, FoodCount, mGameTable));
        }

        #endregion

        #region events

        public event EventHandler<GameEventArgs> Advanced;

        public event EventHandler<GameEventArgs> Over;

        public event EventHandler<GameEventArgs> PlayerMove;

        public event EventHandler<GameEventArgs> Refresh;

        public event EventHandler<GameEventArgs> Created;

        #endregion
    }
}