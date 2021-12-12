using System;

namespace beadXamarin.Persistence
{
    public class GameTree : GameObject
    {
        public GameTree(int m, int n)
        {
            mPosition = new Tuple<int, int>(m, n);
        }
    }
}