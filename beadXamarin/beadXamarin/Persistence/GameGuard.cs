using System;

namespace beadXamarin.Persistence
{
    public enum GameDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public class GameGuard : GameObject
    {
        public GameGuard(int m, int n)
        {
            if ((m + n) % 2 == 0)
                Direction = GameDirection.Up;
            else
                Direction = GameDirection.Right;
            mPosition = new Tuple<int, int>(m, n);
        }

        public GameDirection Direction { get; set; }
    }
}