using System;
using System.Collections.Generic;

namespace beadXamarin.Persistence
{
    public class GameTable
    {
        #region constructors

        public GameTable(int m, int n)
        {
            if (m < 1 || n < 1) throw new ArgumentOutOfRangeException("invalid table size");

            this.M = m;
            this.N = n;
            TableCharRepresentation = new char[this.M, this.N];
            NumOfGuards = 0;
            NumOfFood = 0;
            Guards = new List<GameGuard>();
            Trees = new List<GameTree>();
            Foods = new List<GameFood>();
            Ended = false;
        }

        public GameTable() : this(10, 10)
        {
        }

        #endregion

        #region methods

        public void setFieldOnInit(int m, int n, char val)
        {
            if (m >= this.M || m < 0 || n >= this.N || n < 0)
                throw new ArgumentOutOfRangeException("invalid coordinates");

            if (val != 'P' && val != 'G' && val != 'T' && val != 'F' && val != 'E')
                throw new ArgumentException("invalid field type");

            TableCharRepresentation[m, n] = val;

            if (val == 'G')
            {
                ++NumOfGuards;
                Guards.Add(new GameGuard(m, n));
            }
            else if (val == 'F')
            {
                ++NumOfFood;
                Foods.Add(new GameFood(m, n));
            }
            else if (val == 'P')
            {
                Player = new GamePlayer(m, n);
            }
            else if (val == 'T')
            {
                Trees.Add(new GameTree(m, n));
            }
        }

        public char GetField(int m, int n)
        {
            return TableCharRepresentation[m, n];
        }

        #endregion

        #region properties

        public Tuple<int, int> Size => new Tuple<int, int>(M, N);

        //E = empty field
        //F = Food
        //G = Guard
        //P = Player
        //T = Tree
        public char[,] TableCharRepresentation { get; set; }
        public int M { get; set; }
        public int N { get; set; }
        public GamePlayer Player { get; set; }
        public List<GameFood> Foods { get; set; }
        public List<GameTree> Trees { get; set; }
        public List<GameGuard> Guards { get; set; }
        public int NumOfGuards { get; set; }
        public int NumOfFood { get; set; }
        public bool Ended { get; set; }

        #endregion
    }
}